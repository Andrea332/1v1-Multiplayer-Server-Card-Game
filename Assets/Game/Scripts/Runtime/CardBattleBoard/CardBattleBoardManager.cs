using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Mirror;
using Unity.Services.Apis;
using Unity.Services.Apis.CloudCode;
using Unity.Services.Apis.CloudSave;
using Unity.Services.Apis.Economy;
using Unity.Services.Apis.Leaderboards;
using Unity.Services.Authentication;
#if UNITY_SERVER
using Unity.Services.Multiplay;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class CardBattleBoardManager : NetworkBehaviour
    {
        public BaseCardBuilder cardBuilder;
        [SerializeField] private BaseCard enemyHandCardPrefab;
        [SerializeField] private TableBattleSlot slotPrefab;
        [SerializeField] private CardTypePresenter cardTypePresenter;
        [SerializeField] private SO_ID item_UGS_Id;
        [SerializeField] private SO_ID item_Sprite_Id;
        [SerializeField] private SO_ID item_Ability_Id;
        [SerializeField] private SO_ID item_Attack_Id;
        [SerializeField] private SO_ID item_PlaceOnGround_Id;
        [SerializeField] private ItemsList slotBgList;
        [SerializeField] private ItemsList cardTemplates;
        [SerializeField] private BaseItemString qgItemId;
        [SerializeField] private BaseItemString supportItemId;
        [SerializeField] private BaseItemString submarineItemId;
        public CardActionsMenuManager cardActionsMenuManager;

        [SerializeField] public List<Transform> enemySlotsPositions;
        [SerializeField] public Transform enemyCardsPositions;

        [SerializeField] public List<Transform> playerSlotsPositions;
        [SerializeField] public Transform playerCardsPositions;
        [SerializeField] public Transform playerDeckCardsPositions;
        [SerializeField] public Transform playerCemeteryCardPosition;

        //Server Data
        [SerializeField] private int waitingForPlayersTimeOutInSeconds;
        private CancellationTokenSource waitingForPlayersCancellationTokenSource;
        [SerializeField] private int exitTimeOnMatchOver;
        [HideInInspector] public BattleTableData battleTableData;
        [HideInInspector] public List<TableInfo> tableInfos = new();
        [HideInInspector] public string currentTurnOwnerId;
        private bool matchStarted;
        public bool matchFinished;

        [SerializeField] private int winnerPoints;
        [SerializeField] private int loserPoints;
        [SerializeField] private int drawPoints;
        private CancellationTokenSource turnCancellationTokenSource;
#if UNITY_SERVER
        private IServerClient serverClient;
#endif

        //Client Data
        //Player
        public TableSlot playerQgSlot;
        [HideInInspector] public List<TableBattleSlot> playerTableSlots = new();
        public List<BaseCard> playerCards = new();
        [HideInInspector] public List<BaseCard> cemeteryCards = new();

        //Enemy
        public GameObject enemyQgSlot;
        [HideInInspector] public List<TableBattleSlot> enemyTableSlots = new();
        public List<BaseCard> enemyCards = new();
        [HideInInspector] public List<BaseCard> enemyHandFakeCards = new();
        [HideInInspector] public GameObject enemyQgCard;

        //Client UI manager
        [SerializeField] private CardBattleBoardPresenters cardBattleBoardPresenters;

        //Client Game Events
        public UnityEvent onMatchStarted;
        public UnityEvent onMatchDenied;
        public UnityEvent onMatchWinned;
        public UnityEvent onMatchLosed;
        public UnityEvent onMatchDrawed;
        public UnityEvent onTurnChanged;
        public UnityEvent onYourTurnStarted;
        public UnityEvent onEnemyTurnStarted;
        public UnityEvent onStopClient;

        #region NetworkBehaviour CallBacks

        public override void OnStartAuthority()
        {

        }

        public override void OnStartServer()
        {
            waitingForPlayersCancellationTokenSource = new();

            ServerWaitForPlayers(waitingForPlayersCancellationTokenSource.Token, waitingForPlayersTimeOutInSeconds).Forget();
        }

        public override void OnStartClient()
        {
            cardBattleBoardPresenters.matchEndedTurnTimer.StartTimerBySeconds(waitingForPlayersTimeOutInSeconds);
            
            ClientIsReady();
        }

        public override void OnStartLocalPlayer()
        {

        }

        public override void OnStopServer()
        {

        }

        public override void OnStopAuthority()
        {

        }

        public override void OnStopClient()
        {
            onStopClient?.Invoke();
        }

        public override void OnStopLocalPlayer()
        {

        }

        #endregion

        #region Server Battle Actions

        [Server]
        private async UniTaskVoid ServerWaitForPlayers(CancellationToken cancellationToken, float waitingTime)
        {
            bool isCanceled = await UniTask.WaitForSeconds(waitingTime, cancellationToken: cancellationToken).SuppressCancellationThrow();
            if (isCanceled)
            {
                Debug.Log("All players connected before the max time");
                return;
            }

            Debug.Log("Not every player connected before max time, server will be shut down");

            ClientRpcMatchDenied(exitTimeOnMatchOver);

            await UniTask.Delay(exitTimeOnMatchOver);

            NetworkServer.DisconnectAll();
        }
        
        [Server]
        private void TryStartMatch()
        {
            if (NetworkServer.connections.Count != NetworkServer.maxConnections) return;

            if (NetworkServer.connections.Any(networkConnectionToClient => !networkConnectionToClient.Value.isReady)) return;
           
            ServerStartMatch();
        }

        [Server]
        private void ServerStartMatch()
        {
            Debug.Log("Match Started");
            if (matchStarted) return;
            waitingForPlayersCancellationTokenSource?.Cancel();
            matchStarted = true;
            var playerToStartIndex = UnityEngine.Random.Range(0, tableInfos.Count);
            var playerToStartTableInfo = tableInfos[playerToStartIndex];
            var turnLifeTimeInSeconds = battleTableData.turnLifeTimeInSeconds;
            turnCancellationTokenSource = ServerSetFirstTurn(playerToStartTableInfo);
            ServerStartTurnTimer(turnCancellationTokenSource.Token, turnLifeTimeInSeconds, playerToStartTableInfo.playerId);
            var networkedTableInfos = TableInfo.TableInfosToStruct(tableInfos);
            ClientRpcStartMatch(networkedTableInfos, playerToStartTableInfo.playerId, turnLifeTimeInSeconds);
        }

        [Server]
        private CancellationTokenSource ServerSetFirstTurn(TableInfo nextTurnPlayerTableInfo)
        {
            currentTurnOwnerId = nextTurnPlayerTableInfo.playerId;

            nextTurnPlayerTableInfo.currentFuel = nextTurnPlayerTableInfo.maxFuel;

            nextTurnPlayerTableInfo.turnCounter++;

            return new();
        }

        [ClientRpc]
        private void ClientRpcStartMatch(NetworkedTableInfo[] networkedTableInfos, string turnOwnerId, int turnTime)
        {
            BuildTable(networkedTableInfos);
            UpdateUI(networkedTableInfos);
            UpdateTurnUI(turnOwnerId, turnTime);
            onMatchStarted?.Invoke();
        }

        [Server]
        private async void ServerStartTurnTimer(CancellationToken cancellationToken, float turnTimeLife, string turnOwnerId)
        {
            bool isCanceled = await UniTask.WaitForSeconds(turnTimeLife, cancellationToken: cancellationToken).SuppressCancellationThrow();

            if (isCanceled)
            {
                Debug.Log("Turn Timer cancelled");
                return;
            }

            PassTurn(turnOwnerId);
        }

        [Command(requiresAuthority = false)]
        private void CommandPassTurn(NetworkConnectionToClient sender = null)
        {
            string currentPlayerId = (string)sender.authenticationData;
            TableInfo tableInfo = tableInfos.Find(x => x.playerId == currentPlayerId);
            if (tableInfo.playerId == currentTurnOwnerId)
            {
                PassTurn(currentTurnOwnerId);
            }
        }

        [Server]
        private CancellationTokenSource ServerSetTurn(TableInfo nextTurnPlayerTableInfo)
        {
            currentTurnOwnerId = nextTurnPlayerTableInfo.playerId;

            nextTurnPlayerTableInfo.currentFuel = nextTurnPlayerTableInfo.maxFuel;

            nextTurnPlayerTableInfo.turnCounter++;

            CardData cardData = DrawCardFromDeck(nextTurnPlayerTableInfo);

            onTurnChanged?.Invoke();

            if (cardData != null)
            {
                ClientRpcDrawCardFromDeck(cardData.cardInventoryId, currentTurnOwnerId);
            }

            return new();
        }

        [Server]
        private void PassTurn(string turnOwnerPlayerId)
        {
            turnCancellationTokenSource?.Cancel();

            TableInfo tableInfoForNextTurn = tableInfos.Find(x => x.playerId != turnOwnerPlayerId);

            if (tableInfoForNextTurn.turnCounter < battleTableData.playersMaxTurns)
            {
                var turnLifeTimeInSeconds = battleTableData.turnLifeTimeInSeconds;
                ResetCardsAttackTimes();
                turnCancellationTokenSource = ServerSetTurn(tableInfoForNextTurn);
                ServerStartTurnTimer(turnCancellationTokenSource.Token, turnLifeTimeInSeconds,
                    tableInfoForNextTurn.playerId);
                var networkedTableInfos = TableInfo.TableInfosToStruct(tableInfos);
                ClientRpcUpdateAllUI(networkedTableInfos, tableInfoForNextTurn.playerId, turnLifeTimeInSeconds);
                return;
            }
            
            CalculateMatchVictory().Forget();
        }

        [Server]
        private async UniTaskVoid CalculateMatchVictory()
        {
            matchFinished = true;
            
            TableInfo winnerTableInfo = tableInfos[0];
            for (int i = 0; i < tableInfos.Count; i++)
            {
                if (tableInfos[i] == winnerTableInfo) continue;
                if (tableInfos[i].points > winnerTableInfo.points)
                {
                    winnerTableInfo = tableInfos[i];
                }
                else if (tableInfos[i].points == winnerTableInfo.points)
                {
                    ClientRpcMatchEndedDraw(exitTimeOnMatchOver);
#if UNITY_SERVER
                    await UpdatePlayersStatsOnDraw(tableInfos, drawPoints);
#endif
                    await AwaitTimeAndStopServer(exitTimeOnMatchOver);
                    
                    return;
                }
            }

            ClientRpcMatchEndedWin(winnerTableInfo.playerId, exitTimeOnMatchOver);
#if UNITY_SERVER
            TableInfo loserTableInfo = tableInfos.Find(x => x != winnerTableInfo);

            await UpdatePlayersStatsOnWin(winnerTableInfo, loserTableInfo, winnerPoints, loserPoints);
#endif
            await AwaitTimeAndStopServer(exitTimeOnMatchOver);
        }

        private async UniTask AwaitTimeAndStopServer(int matchExitTime)
        {
            await UniTask.WaitForSeconds(matchExitTime);

            NetworkServer.DisconnectAll();
        }

        [ClientRpc]
        private void ClientRpcMatchEndedWin(string winnerId, int matchExitTime)
        {
            cardBattleBoardPresenters.matchEndedTurnTimer.StartTimerBySeconds(matchExitTime);

            if (winnerId == AuthenticationService.Instance.PlayerId)
            {
                onMatchWinned?.Invoke();
                return;
            }

            onMatchLosed?.Invoke();
        }

        [ClientRpc]
        private void ClientRpcMatchDenied(int matchExitTime)
        {
            MatchDenied(matchExitTime);
        }

        private void MatchDenied(int matchExitTime)
        {
            cardBattleBoardPresenters.matchEndedTurnTimer.StartTimerBySeconds(matchExitTime);
            onMatchDenied?.Invoke();
        }

        [ClientRpc]
        private void ClientRpcMatchEndedDraw(int matchExitTime)
        {
            cardBattleBoardPresenters.matchEndedTurnTimer.StartTimerBySeconds(matchExitTime);
            onMatchDrawed?.Invoke();
        }

        private void ResetCardsAttackTimes()
        {
            foreach (var tableInfo in tableInfos)
            {
                foreach (var battleSlotInfo in tableInfo.slotInfos)
                {
                    if(battleSlotInfo.slotCardData == null) continue;
                    
                    battleSlotInfo.slotCardData.currentAttackTimes = 0;
                }
            }
        }

        [Server]
        public async UniTaskVoid OnPlayerDisconnected(string playerId)
        {
            if (!matchStarted)
            {
                Debug.Log($"Player {playerId} disconnected on match not started");
                
                ClientRpcMatchDenied(exitTimeOnMatchOver);
                
                await AwaitTimeAndStopServer(exitTimeOnMatchOver);
                
                return;
            }
            
            Debug.Log($"Player {playerId} disconnected on match started");

            TableInfo winnerTableInfo = tableInfos.Find(x => x.playerId != playerId);
            ClientRpcMatchEndedWin(winnerTableInfo.playerId, exitTimeOnMatchOver);
#if UNITY_SERVER
            TableInfo loserTableInfo = tableInfos.Find(x => x != winnerTableInfo);
            
            await UpdatePlayersStatsOnWin(winnerTableInfo, loserTableInfo, winnerPoints, loserPoints);
#endif
            await AwaitTimeAndStopServer(exitTimeOnMatchOver);
        }
        
        [Command(requiresAuthority = false)]
        private void ClientIsReady()
        {
            TryStartMatch();
        }

        #region UI Update

        [ClientRpc]
        private void ClientRpcUpdateAllUI(NetworkedTableInfo[] networkedTableInfos, string turnOwnerId, int turnTime)
        {
            UpdateUI(networkedTableInfos);
            UpdateTurnUI(turnOwnerId, turnTime);
        }

        [ClientRpc]
        public void ClientRpcUpdateUI(NetworkedTableInfo[] networkedTableInfos)
        {
            UpdateUI(networkedTableInfos);
        }

        #endregion
        
        [ClientRpc]
        public void PingToClient(string message)
        {
            Debug.Log("Server Message: " + message);
        }

      
        #region Place BattleCard On Slot
        public void OnCardDragToFindSlotBegin(BaseCard card)
        {
            var cardEconomyId = card.GetComponent<CardEconomyIdComponent>().Parameter;
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, cardEconomyId);

            BaseCardPlaceOnGround baseCardPlaceOnGround = cardItem.GetItem<BaseCardPlaceOnGround>(item_PlaceOnGround_Id.id);
            
            if (baseCardPlaceOnGround == null)
            {
                string message = $"Place on ground of card {cardItem.name} not found";
                PingToClient(message);
                Debug.Log(message);
                return;
            }
            
            baseCardPlaceOnGround.ClientPreparePlaceOnGround(this, card);
        }
        
        public void OnCardDragToFindSlotEnd(BaseCard card)
        {
            string cardEconomyId = card.GetComponent<CardEconomyIdComponent>().Parameter;
            
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, cardEconomyId);

            BaseCardPlaceOnGround baseCardPlaceOnGround = cardItem.GetItem<BaseCardPlaceOnGround>(item_PlaceOnGround_Id.id);
            
            baseCardPlaceOnGround.DespawnAvailabilityManager(card);
        }

        public void ClientTryPlaceCard(int slotId, string slotOwnerId, BaseCard card, string cardPlayerOwnerId)
        {
            string cardEconomyId = card.GetComponent<CardEconomyIdComponent>().Parameter;
            string cardInventoryId = card.GetComponent<CardInventoryIdComponent>().Parameter;
            string cardType = card.GetComponent<CardTypeComponent>().Parameter;
            
            CmdTryPlaceCard(slotId, slotOwnerId, cardEconomyId, cardInventoryId, cardPlayerOwnerId, cardType);
        }

        [Command(requiresAuthority = false)]
        public void CmdTryPlaceCard(int slotId, string slotOwnerId, string cardEconomyId, string cardInventoryId, string cardPlayerOwnerId, string cardType, NetworkConnectionToClient sender = null)
        {
            if ((string)sender.authenticationData != currentTurnOwnerId) return;
            
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, cardEconomyId);

            BaseCardPlaceOnGround baseCardPlaceOnGround = cardItem.GetItem<BaseCardPlaceOnGround>(item_PlaceOnGround_Id.id);
            
            if (baseCardPlaceOnGround == null)
            {
                string message = $"Place on ground of card {cardItem.name} not found";
                PingToClient(message);
                Debug.Log(message);
                return;
            }
        
            baseCardPlaceOnGround.ServerTryPlaceCardOnGround(this, slotId, slotOwnerId, cardInventoryId, cardPlayerOwnerId, cardType);
        }
        
        [ClientRpc]
        public void ClientRpcPlaceCard(int slotId, string slotOwnerId, NetworkedCardData networkedCardData)
        {
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, networkedCardData.cardEconomyId);

            BaseCardPlaceOnGround baseCardPlaceOnGround = cardItem.GetItem<BaseCardPlaceOnGround>(item_PlaceOnGround_Id.id);
            
            if (baseCardPlaceOnGround == null)
            {
                string message = $"Place on ground of card {cardItem.name} not found";
                PingToClient(message);
                Debug.Log(message);
                return;
            }
        
            baseCardPlaceOnGround.ClientPlaceCard(this, slotId, slotOwnerId, networkedCardData);
        }
        
        [ClientRpc]
        public void ClientRpcDenySetSlotWithCard(int slotId, string slotOwnerId, string cardInventoryId)
        {
            if (slotOwnerId != AuthenticationService.Instance.PlayerId) return;
            
            BattleCard battleCardToSet = (BattleCard)playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == cardInventoryId);
            TableBattleSlot tableSlot = playerTableSlots.Find(x => x.Id == slotId);
            if (battleCardToSet.GetComponent<CardTypeComponent>().Parameter == supportItemId.ItemValue)
            {
                tableSlot.UnSetSupportCard(battleCardToSet);
                return;
            }

            tableSlot.UnSetCard(battleCardToSet);
        }

        #endregion

        #region Draw BattleCard From Player Deck

        [Server]
        private CardData DrawCardFromDeck(TableInfo tableInfo)
        {
            if (tableInfo.deckCardDatas.Count == 0)
            {
                string message = $"Player {tableInfo.playerId} has finished deck cards";
                PingToClient(message);
                Debug.Log(message);
                return null;
            }

            var randomCardIndex = UnityEngine.Random.Range(0, tableInfo.deckCardDatas.Count);
            var cardDataSelected = tableInfo.deckCardDatas[randomCardIndex];
            tableInfo.handCardDatas.Add(cardDataSelected);
            tableInfo.deckCardDatas.RemoveAt(randomCardIndex);
            return cardDataSelected;
        }

        [ClientRpc]
        private void ClientRpcDrawCardFromDeck(string cardInventoryId, string nextTurnPlayerId)
        {
            if (nextTurnPlayerId == AuthenticationService.Instance.PlayerId)
            {
                BattleCard battleCardToSet = (BattleCard)playerCards.Find(x => x.GetComponent<CardInventoryIdComponent>().Parameter == cardInventoryId);
                battleCardToSet.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
                DeckCardsPresenter deckCardsPresenter = playerCardsPositions.GetComponent<DeckCardsPresenter>();
                deckCardsPresenter.AddCard(battleCardToSet, true);
                return;
            }

            BuildEnemyFakeCard(enemyCardsPositions);
        }

        #endregion

        #region Attack Enemy BattleCard By Another BattleCard
        
        [ClientRpc]
        public void ClientRpcDestroySlotMainCard(int receiverSlotId, string receiverSlotOwnerId)
        {
            TableSlot tableSlot;
            if (receiverSlotOwnerId == AuthenticationService.Instance.PlayerId)
            {
                tableSlot = playerTableSlots.Find(x => x.Id == receiverSlotId);
                if(tableSlot.CurrentBattleCard.TryGetComponent(out IAbilityAble abilityAble))
                {
                    abilityAble.RemoveAbilityUsed();
                }
                playerCards.Remove(tableSlot.CurrentBattleCard);
                tableSlot.CurrentBattleCard.transform.SetParent(playerCemeteryCardPosition, false);
                cemeteryCards.Add(tableSlot.CurrentBattleCard);
            }
            else
            {
                tableSlot = enemyTableSlots.Find(x => x.Id == receiverSlotId);
                if(tableSlot.CurrentBattleCard.TryGetComponent(out IAbilityAble abilityAble))
                {
                    abilityAble.RemoveAbilityUsed();
                }
                enemyCards.Remove(tableSlot.CurrentBattleCard);
                Destroy(tableSlot.CurrentBattleCard.gameObject);
            }
            tableSlot.UnSetCard();
        }
        
        [ClientRpc]
        public void ClientRpcDestroySlotSupportCard(int receiverSlotId, string receiverSlotOwnerId)
        {
            TableBattleSlot tableSlot;
            if (receiverSlotOwnerId == AuthenticationService.Instance.PlayerId)
            {
                tableSlot = playerTableSlots.Find(x => x.Id == receiverSlotId);
                if(tableSlot.CurrentSupportCard.TryGetComponent(out IAbilityAble abilityAble))
                {
                    abilityAble.RemoveAbilityUsed();
                }
                playerCards.Remove(tableSlot.CurrentSupportCard);
                tableSlot.CurrentSupportCard.transform.SetParent(playerCemeteryCardPosition, false);
                cemeteryCards.Add(tableSlot.CurrentSupportCard);
            }
            else
            {
                tableSlot = enemyTableSlots.Find(x => x.Id == receiverSlotId);
                if(tableSlot.CurrentSupportCard.TryGetComponent(out IAbilityAble abilityAble))
                {
                    abilityAble.RemoveAbilityUsed();
                }
                enemyCards.Remove(tableSlot.CurrentSupportCard);
                Destroy(tableSlot.CurrentSupportCard.gameObject);
            }
            tableSlot.UnSetSupportCard();
        }

        [Command(requiresAuthority = false)]
        public void CmdTryAttackSlot(int receiverSlotId, string receiverSlotOwnerId, string attackerCardEconomyId, string attackerCardInventoryId, string attackerPlayerId, NetworkConnectionToClient sender = null)
        {
            if ((string)sender.authenticationData != currentTurnOwnerId) return;
            
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, attackerCardEconomyId);

            BaseCardAttack baseCardAttack = cardItem.GetItem<BaseCardAttack>(item_Attack_Id.id);
            
            if (baseCardAttack == null)
            {
                string message = $"Attack of card {cardItem.name} not found";
                PingToClient(message);
                Debug.Log(message);
                return;
            }
        
            baseCardAttack.ServerTryAttackCard(this, receiverSlotId, receiverSlotOwnerId, attackerCardInventoryId, attackerPlayerId);
        }
        
        [ClientRpc]
        public void ClientRpcAttackSlot(int receiverSlotId, string receiverSlotOwnerId, string attackerCardEconomyId, int damage)
        {
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, attackerCardEconomyId);

            BaseCardAttack baseCardAttack = cardItem.GetItem<BaseCardAttack>(item_Attack_Id.id);
            
            if (baseCardAttack == null)
            {
                string message = $"Attack of card {cardItem.name} not found";
                Debug.Log(message);
                return;
            }

            baseCardAttack.ClientAttackCard(this, receiverSlotId, receiverSlotOwnerId,  damage);
        }
        
        #endregion
        
        #region Execute Card Ability

        public void TryStartPrepareAbility(CardData cardData, string cardOwnerId)
        {
            BasePropertiesItem cardTemplate = cardTemplates.FindPropertiesItem(item_UGS_Id.id, cardData.cardEconomyId);

            BaseCardAbility baseCardAbility = cardTemplate.GetItem<BaseCardAbility>(item_Ability_Id.id);

            if (baseCardAbility == null) return;
            
            if (baseCardAbility.startOnDeployed)
            {
                baseCardAbility.ServerPrepareAbility(this, cardData.cardEconomyId, cardData.cardInventoryId, cardOwnerId);
            }
        }
        
        [Client]
        public void PrepareAbility(string cardEconomyId, string cardInventoryId)
        {
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, cardEconomyId);
            
            BaseCardAbility baseCardAbility = cardItem.GetItem<BaseCardAbility>(item_Ability_Id.id);
            
            if (baseCardAbility == null)
            {
                string message = $"Ability of card {cardItem.name} not found";
                PingToClient(message);
                Debug.Log(message);
                return;
            }
            
            baseCardAbility.ClientPrepareAbility(this, cardEconomyId, cardInventoryId);
        }
        
        [Client]
        public void ClientExecuteCardAbility(string cardEconomyId, string cardInventoryId, string parameter)
        {
            CmdExecuteCardAbility(cardEconomyId, cardInventoryId, parameter);
        }
        [Command(requiresAuthority = false)]
        public void CmdExecuteCardAbility(string cardEconomyId, string cardInventoryId, string serverJsonParameter, NetworkConnectionToClient sender = null)
        {
            if ((string)sender.authenticationData != currentTurnOwnerId) return;
            
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, cardEconomyId);

            BaseCardAbility baseCardAbility = cardItem.GetItem<BaseCardAbility>(item_Ability_Id.id);
            
            if (baseCardAbility == null)
            {
                string message = $"Ability of card {cardItem.name} not found";
                PingToClient(message);
                Debug.Log(message);
                return;
            }
        
            baseCardAbility.ServerDoAbility(this, cardEconomyId, cardInventoryId, serverJsonParameter);
            ClientRpcUpdateUI(TableInfo.TableInfosToStruct(tableInfos));
        }
        
        [ClientRpc]
        public void ClientRpcExecuteCardAbility(string jsonParameter, string cardEconomyId)
        {
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, cardEconomyId);

            BaseCardAbility baseCardAbility = cardItem.GetItem<BaseCardAbility>(item_Ability_Id.id);
            
            baseCardAbility.ClientDoAbility(this, jsonParameter);
        }

        #endregion
        
        #endregion

        #region Client Battle Actions

        [Client]
        public void ClientPassTurn()
        {
            CommandPassTurn();
        }

        #endregion

#if UNITY_SERVER
        #region Server Game Mode Setup
        
        [Server]
        public async Task Inizialize(UgsServerManager ugsServerManager)
        {
            try
            {
                serverClient = ugsServerManager.serverClient;
            
                string moduleName = ProjectKeys.CC_MODULE;
                string moduleFunctionName = ProjectKeys.CC_FUNCTION_GETBATTLEDATA;
                var runModule = await serverClient.CloudCode.RunModule(ProjectConfigurations.ServerProjectId, moduleName, moduleFunctionName).AsTask;
                battleTableData = runModule.Data.Output.GetAs<BattleTableData>();
            
                foreach (var player in ugsServerManager.matchmakingResults.MatchProperties.Players)
                {
                    long deckNumber = (long)player.CustomData.GetAs<Dictionary<string, object>>()[MatchmakingSO.deckNumberDictionaryName];
                    DeckData deckData = await ServerLoadPlayerDeck(player.Id, deckNumber, serverClient);
                    List<CardData> cardDatas = await ServerLoadPlayerDeckCardsDatas(player.Id, deckData, serverClient);
                    MatchesStatistics matchesStatistics = await ServerLoadPlayersMatchStats(player.Id, serverClient);
                    var playerName = await serverClient.PlayerNames.GetName(player.Id).AsTask;
                    ServerSpawnTableForPlayer(cardDatas, player.Id, playerName.Data.Name, matchesStatistics, battleTableData);
                }

                await MultiplayService.Instance.ReadyServerForPlayersAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                
                await MultiplayService.Instance.UnreadyServerAsync();
                
                Application.Quit();
            }
        }
        
        [Server]
        private void ServerSpawnTableForPlayer(List<CardData> deckCardDatas, string playerId, string playerName, MatchesStatistics matchesStatistics, BattleTableData battleTableData)
        {
            SlotInfo qgSlotInfo = new()
            {
                id = 99,
                slotData = new SlotData()
                {
                    types = new (),
                    bgImage = string.Empty
                }
            };
            
            TableInfo tableInfo = new(
                playerId,
                playerName,
                matchesStatistics,
                battleTableData.playersMaxFuel,
                battleTableData.playersMaxFuel,
                qgSlotInfo
            );

            if (tableInfos == null)
            {
                tableInfos = new();
            }

            tableInfos.Add(tableInfo);

            SpawnCardsForPlayer(deckCardDatas, playerId, battleTableData.playersHandCardsAtStart);

            SpawnSlotsForPlayer(battleTableData.battlePlayerSlots, playerId);
        }

        [Server]
        private void SpawnSlotsForPlayer(List<SlotData> slotDatas, string playerId)
        {
            TableInfo tableInfo = tableInfos.Find(x => x.playerId == playerId);
            
            for (int i = 0; i < slotDatas.Count; i++)
            {
                var slotData = slotDatas[i];
                BattleSlotInfo tableSlotInfo = new()
                {
                    id = i,
                    slotData = slotData
                };
             
                tableInfo.slotInfos.Add(tableSlotInfo);
            }
        }

        [Server]
        private void SpawnCardsForPlayer(List<CardData> deckCardDatas, string playerId, int handCardsAtStart)
        {
            TableInfo tableInfo = tableInfos.Find(x => x.playerId == playerId);

            foreach (var deckCardData in deckCardDatas)
            {
                deckCardData.defaultHealth = deckCardData.health;
                deckCardData.defaultAttack = deckCardData.attack;
                deckCardData.defaultCost = deckCardData.cost;
                deckCardData.defaultTargetType = deckCardData.targetType;
            }

            List<CardData> currentMatchDeckCards = new();
            currentMatchDeckCards.AddRange(deckCardDatas);
            
            tableInfo.qgSlotInfo.slotCardData = currentMatchDeckCards.Find(x => x.rarity == qgItemId.ItemValue);

            if (tableInfo.qgSlotInfo.slotCardData != null)
            {
                currentMatchDeckCards.Remove(tableInfo.qgSlotInfo.slotCardData);
            }
            
            List<CardData> handCards = new();
          
            for (int i = 0; i < handCardsAtStart; i++)
            {
                int randomCardIndex = UnityEngine.Random.Range(0, currentMatchDeckCards.Count);
                handCards.Add(currentMatchDeckCards[randomCardIndex]);
                currentMatchDeckCards.Remove(currentMatchDeckCards[randomCardIndex]);
            }

            tableInfo.handCardDatas = handCards;
            tableInfo.deckCardDatas = currentMatchDeckCards;
        }
        
        [Server]
        private async Task<MatchesStatistics> ServerLoadPlayersMatchStats(string playerId, IServerClient serverClient)
        {
            try
            {
                List<string> saveKeys = new(){ ProjectKeys.MATCHESSTATS_CS_KEY };
            
                var getItemsResponse = await serverClient.CloudSaveData.GetItems(ProjectConfigurations.ServerProjectId, playerId, saveKeys).AsTask;
                
                var items = getItemsResponse.Data.Results;

                var item = items.Find(x => x.Key == ProjectKeys.MATCHESSTATS_CS_KEY);

                if (item != null)
                {
                    return JsonUtility.FromJson<MatchesStatistics>(item.Value.ToString());
                }
            
                return default;
            }
            catch (Exception e)
            {
                Debug.LogError("UGS Get players match statistics error: " + e.Message);

                await MultiplayService.Instance.UnreadyServerAsync();
                
                Application.Quit();

                return default;
            }
        }
        [Server]
        private async Task<DeckData> ServerLoadPlayerDeck(string playerId, long deckNumber, IServerClient serverClient)
        {
            List<string> saveKeys = new(){ ProjectKeys.DECKS_SAVE_CLOUDSAVE_KEY };
            
            var getItemsResponse = await serverClient.CloudSaveData.GetItems(ProjectConfigurations.ServerProjectId, playerId, saveKeys).AsTask;
            
            if (getItemsResponse.IsSuccessful)
            {
                var items = getItemsResponse.Data.Results;

                var item = items.Find(x => x.Key == ProjectKeys.DECKS_SAVE_CLOUDSAVE_KEY);

                DecksData decksData = JsonUtility.FromJson<DecksData>(item.Value.ToString());

                return decksData.decksData.Find(x => x.deckNumber == deckNumber);
            }
            
            Debug.LogError("UGS Get player decks error: " + getItemsResponse.ErrorType);

            return null;
        }

        [Server]
        private async Task<List<CardData>> ServerLoadPlayerDeckCardsDatas(string playerId, DeckData deckData, IServerClient serverClient)
        {
            List<CardData> playerInventoryCardsDatas = new();
            
            ApiResponse<PlayerConfigurationResponse> playerEconomyConfigurationResponse = await serverClient.EconomyConfiguration.GetPlayerConfiguration(ProjectConfigurations.ServerProjectId, playerId).AsTask;

            var playerEconomyConfiguration = playerEconomyConfigurationResponse.Data.Results;
            
            for (int i = 0; i < deckData.cards.Count; i++)
            {
                var playerConfigurationResponseResultsInner = playerEconomyConfiguration.Find(x => x.GetInventoryItemResource().Id == deckData.cards[i].cardEconomyId);
                InventoryItemResource cardItem = playerConfigurationResponseResultsInner.GetInventoryItemResource();
                var cardData = JsonUtility.FromJson<CardData>(cardItem.CustomData.ToString());
                cardData.cardName = cardItem.Name;
                cardData.cardEconomyId = deckData.cards[i].cardEconomyId;
                cardData.cardInventoryId = deckData.cards[i].cardInventoryId;
                playerInventoryCardsDatas.Add(cardData);
            }
            
            return playerInventoryCardsDatas;
        }

        [Server]
        private async Task UpdatePlayersStatsOnWin(TableInfo winnerTableInfo, TableInfo loserTableInfo, int winnerPointsToAssign, int loserPointsToAssign)
        {
            var winnerMatchesStatistics = winnerTableInfo.matchesStatistics;
            winnerMatchesStatistics.matchesWon += 1;
            
            Dictionary<string, object> winnerDictionary = new() { { ProjectKeys.CC_FUNCTION_SAVEMATCHSTATS_MATCHSTAT_PARAMETER, winnerMatchesStatistics },{ProjectKeys.CC_FUNCTION_SAVEMATCHSTATS_PLAYERID_PARAMETER,winnerTableInfo.playerId} };
            var winnerRunModuleArguments = new RunModuleArguments(winnerDictionary);
            await serverClient.CloudCode.RunModule(ProjectConfigurations.ServerProjectId, ProjectKeys.CC_MODULE, ProjectKeys.CC_FUNCTION_SAVEMATCHSTATS, winnerRunModuleArguments);
            
            LeaderboardMetaData winnerMetaData = new( winnerMatchesStatistics.matchesDraw, winnerMatchesStatistics.matchesWon, winnerMatchesStatistics.matchesLost);
            AddLeaderboardScore winnerAddLeaderboardScore = new AddLeaderboardScore(winnerPointsToAssign, winnerMetaData);
            await serverClient.Leaderboards.AddLeaderboardPlayerScore(ProjectConfigurations.ServerProjectId, ProjectKeys.LEADERBOARD_BATTLEPOINTS, winnerTableInfo.playerId, winnerAddLeaderboardScore);
            
            var loserMatchesStatistics = loserTableInfo.matchesStatistics;
            loserMatchesStatistics.matchesLost += 1;
            
            Dictionary<string, object> loserDictionary = new() { { ProjectKeys.CC_FUNCTION_SAVEMATCHSTATS_MATCHSTAT_PARAMETER, winnerMatchesStatistics },{ProjectKeys.CC_FUNCTION_SAVEMATCHSTATS_PLAYERID_PARAMETER,loserTableInfo.playerId} };
            var loserRunModuleArguments = new RunModuleArguments(loserDictionary);
            await serverClient.CloudCode.RunModule(ProjectConfigurations.ServerProjectId, ProjectKeys.CC_MODULE, ProjectKeys.CC_FUNCTION_SAVEMATCHSTATS, loserRunModuleArguments);
            
            LeaderboardMetaData loserMetaData = new(loserMatchesStatistics.matchesDraw, loserMatchesStatistics.matchesWon, loserMatchesStatistics.matchesLost);
            AddLeaderboardScore loserAddLeaderboardScore = new AddLeaderboardScore(loserPointsToAssign, loserMetaData);
            await serverClient.Leaderboards.AddLeaderboardPlayerScore(ProjectConfigurations.ServerProjectId, ProjectKeys.LEADERBOARD_BATTLEPOINTS, loserTableInfo.playerId, loserAddLeaderboardScore);
        }
        [Server]
        private async Task UpdatePlayersStatsOnDraw(List<TableInfo> playersTableInfo, int drawPointsToAssign)
        {
            foreach (var currentTableInfo in playersTableInfo)
            {
                var drawerMatchesStatistics = currentTableInfo.matchesStatistics;
                drawerMatchesStatistics.matchesDraw += 1;

                Dictionary<string, object> dictionary = new() { { ProjectKeys.CC_FUNCTION_SAVEMATCHSTATS_MATCHSTAT_PARAMETER, drawerMatchesStatistics },{ProjectKeys.CC_FUNCTION_SAVEMATCHSTATS_PLAYERID_PARAMETER,currentTableInfo.playerId} };
                var runModuleArguments = new RunModuleArguments(dictionary);
                await serverClient.CloudCode.RunModule(ProjectConfigurations.ServerProjectId, ProjectKeys.CC_MODULE, ProjectKeys.CC_FUNCTION_SAVEMATCHSTATS, runModuleArguments);
                
                LeaderboardMetaData drawMetaData = new(drawerMatchesStatistics.matchesDraw, drawerMatchesStatistics.matchesWon, drawerMatchesStatistics.matchesLost);
                AddLeaderboardScore addLeaderboardScore = new AddLeaderboardScore(drawPointsToAssign, drawMetaData);
                await serverClient.Leaderboards.AddLeaderboardPlayerScore(ProjectConfigurations.ServerProjectId, ProjectKeys.LEADERBOARD_BATTLEPOINTS, currentTableInfo.playerId, addLeaderboardScore);
            }
        }

        #endregion
#endif
        #region Client Game Mode Setup
        
        [Client]
        private void BuildTable(NetworkedTableInfo[] networkedTableInfos)
        {
            string currentPlayerId = AuthenticationService.Instance.PlayerId;
            
            NetworkedTableInfo playerTableInfo = networkedTableInfos.ToList().Find(x => x.playerId == currentPlayerId);
            
            NetworkedTableInfo enemyTableInfo = networkedTableInfos.ToList().Find(x => x.playerId != currentPlayerId);
            
            BuildPlayerSlots(playerSlotsPositions, playerTableInfo, playerTableSlots);
            
            BuildEnemySlots(enemySlotsPositions, enemyTableInfo, enemyTableSlots);

            BuildPlayerQgCard(playerTableInfo.qgSlotInfo.slotCardData, playerQgSlot);
            
            BuildEnemyQgCards(enemyTableInfo.qgSlotInfo.slotCardData, enemyQgSlot);
        
            BuildHandCards(playerTableInfo.handCardDatas, playerCardsPositions);
            
            BuildDeckCards(playerTableInfo.deckCardDatas, playerDeckCardsPositions);
            
            BuildEnemyHandCards(enemyTableInfo.handCardDatas, enemyCardsPositions);
        }

        [Client]
        private void BuildPlayerSlots(List<Transform> slotsParents, NetworkedTableInfo networkedTableInfo, List<TableBattleSlot> tableSlotsToUse)
        {
            for (int i = 0; i < networkedTableInfo.slotInfos.Length; i++)
            {
                TableBattleSlot slotInstantiated = Instantiate(slotPrefab, slotsParents[i]);
                slotInstantiated.CardBattleBoardManager = this;
                slotInstantiated.Id = networkedTableInfo.slotInfos[i].id;
                slotInstantiated.PlayerOwnerId = networkedTableInfo.playerId;
                TrySetSlotBgImage(networkedTableInfo.slotInfos[i].slotData.bgImage, slotInstantiated.gameObject);
                TrySetSlotTypes(networkedTableInfo.slotInfos[i].slotData.types, slotInstantiated.gameObject);
                slotInstantiated.onSlotClicked.AddListener(cardActionsMenuManager.SetMenu);
                tableSlotsToUse.Add(slotInstantiated);
            }
        }
        [Client]
        private void BuildEnemySlots(List<Transform> slotsParents, NetworkedTableInfo networkedTableInfo, List<TableBattleSlot> tableSlotsToUse)
        {
            for (int i = 0; i < networkedTableInfo.slotInfos.Length; i++)
            {
                TableBattleSlot slotInstantiated = Instantiate(slotPrefab, slotsParents[i]);
                slotInstantiated.CardBattleBoardManager = this;
                slotInstantiated.Id = networkedTableInfo.slotInfos[i].id;
                slotInstantiated.PlayerOwnerId = networkedTableInfo.playerId;
                TrySetSlotBgImage(networkedTableInfo.slotInfos[i].slotData.bgImage, slotInstantiated.gameObject);
                TrySetSlotTypes(networkedTableInfo.slotInfos[i].slotData.types, slotInstantiated.gameObject);
                tableSlotsToUse.Add(slotInstantiated);
            }
        }
        
        [Client]
        private void TrySetSlotTypes(string[] types, GameObject slotInstantiated)
        {
            foreach (var slotType in types)
            {
                CardTypeComponent cardTypeComponent = slotInstantiated.AddComponent<CardTypeComponent>();
                HorizontalLayoutGroup horizontalLayoutGroup = slotInstantiated.GetComponentInChildren<HorizontalLayoutGroup>();
                CardTypePresenter currentCardTypePresenter = Instantiate(cardTypePresenter, horizontalLayoutGroup.transform);
                cardTypeComponent.onParameterChanged = new();
                cardTypeComponent.onParameterChanged.AddListener(currentCardTypePresenter.SetParameter);
                cardTypeComponent.Parameter = slotType;
            }
        }
        
        [Client]
        private void TrySetSlotBgImage(string bgImageId, GameObject slotInstantiated)
        {
            if (string.IsNullOrEmpty(bgImageId)) return;
            
            BasePropertiesItem basePropertiesItem = slotBgList.FindPropertiesItem(item_UGS_Id.id, bgImageId);
            Sprite currentSprite = basePropertiesItem.GetItemValue<Sprite>(item_Sprite_Id.id);
            Transform imageBgGameObject = slotInstantiated.transform.GetChild(0);
            Image image = imageBgGameObject.GetComponent<Image>();
            image.sprite = currentSprite;
            image.gameObject.SetActive(true);
        }
        
        [Client]
        private void BuildPlayerQgCard(NetworkedCardData qgCardData, TableSlot qgTableSlot)
        {
            if(string.IsNullOrEmpty(qgCardData.cardInventoryId)) return;
            
            var playerQgCard = cardBuilder.BuildCardWithReturnType(qgCardData);
            
            playerCards.Add(playerQgCard);
            
            qgTableSlot.onSlotClicked.AddListener(cardActionsMenuManager.SetMenu);
            
            playerQgCard.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
            
            qgTableSlot.SetCard(playerQgCard);

            RectTransform qgCardRectTransform = playerQgCard.GetComponent<RectTransform>();
            
            qgCardRectTransform.anchorMin = Vector2.zero;
            qgCardRectTransform.anchorMax = Vector2.one;
            qgCardRectTransform.localPosition = Vector3.zero;
            qgCardRectTransform.anchoredPosition = Vector2.zero;
            qgCardRectTransform.sizeDelta = Vector2.zero;
        }
        
        [Client]
        private void BuildEnemyQgCards(NetworkedCardData qgCardData, GameObject qgTableSLot)
        {
            if(string.IsNullOrEmpty(qgCardData.cardInventoryId)) return;
            
            enemyQgCard = Instantiate(enemyHandCardPrefab.gameObject, qgTableSLot.transform, false);
            
            RectTransform qgCardRectTransform = enemyQgCard.GetComponent<RectTransform>();
            
            qgCardRectTransform.anchorMin = Vector2.zero;
            qgCardRectTransform.anchorMax = Vector2.one;
            qgCardRectTransform.localPosition = Vector3.zero;
            qgCardRectTransform.anchoredPosition = Vector2.zero;
            qgCardRectTransform.sizeDelta = Vector2.zero;
        }
        
        [Client]
        private void BuildHandCards(NetworkedCardData[] cardDatas, Transform cardsParent)
        {
            List<BaseCard> cards = cardBuilder.BuildCardsWithReturnType(cardDatas);
            
            foreach (var card in cards)
            {
                card.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
            }
     
            DeckCardsPresenter deckCardsPresenter = cardsParent.GetComponent<DeckCardsPresenter>();
            
            deckCardsPresenter.AddCards(cards);

            playerCards.AddRange(cards);
        }
        
        [Client]
        private void BuildDeckCards(NetworkedCardData[] cardDatas, Transform cardsParent)
        {
            List<BaseCard> cards = cardBuilder.BuildCardsWithReturnType(cardDatas);

            foreach (var card in cards)
            {
                card.GetComponentInChildren<MaxDeckablePresenter>(true).gameObject.SetActive(false);
                card.transform.SetParent(cardsParent, false);
                card.gameObject.SetActive(false);
            }

            playerCards.AddRange(cards);
        }
        
        [Client]
        private void BuildEnemyHandCards(NetworkedCardData[] cardDatas, Transform cardsParent)
        {
            for (int i = 0; i < cardDatas.Length; i++)
            {
                BuildEnemyFakeCard(cardsParent);
            }
        }
        
        [Client]
        private void BuildEnemyFakeCard(Transform cardsParent)
        {
            BaseCard fakeCard = Instantiate(enemyHandCardPrefab, cardsParent);
            enemyHandFakeCards.Add(fakeCard);
        }

        #endregion

        #region Client UI 

        [Client]
        public void UpdateUI(NetworkedTableInfo[] networkedTableInfos)
        {
            //Get current PlayerId
            string currentPlayerId = AuthenticationService.Instance.PlayerId;
            //Set Player UI
            NetworkedTableInfo playerTableInfo = networkedTableInfos.ToList().Find(x=>x.playerId == currentPlayerId);
            cardBattleBoardPresenters.playerNameUI.SetParameter(playerTableInfo.playerName);
            cardBattleBoardPresenters.playerPointsUI.SetParameter(playerTableInfo.points);
            cardBattleBoardPresenters.playerFuelUI.SetParameter(playerTableInfo.currentFuel);
            cardBattleBoardPresenters.playerMaxFuelUI.SetParameter(playerTableInfo.maxFuel);
            cardBattleBoardPresenters.playerTurnNumber.SetParameter(playerTableInfo.turnCounter);
            //Set Enemy UI
            NetworkedTableInfo enemyTableInfo = networkedTableInfos.ToList().Find(x=>x.playerId != currentPlayerId);
            cardBattleBoardPresenters.enemyNameUI.SetParameter(enemyTableInfo.playerName);
            cardBattleBoardPresenters.enemyPointsUI.SetParameter(enemyTableInfo.points);
            cardBattleBoardPresenters.enemyFuelUI.SetParameter(enemyTableInfo.currentFuel);
            cardBattleBoardPresenters.enemyMaxFuelUI.SetParameter(enemyTableInfo.maxFuel);
            cardBattleBoardPresenters.enemyTurnNumber.SetParameter(enemyTableInfo.turnCounter);
        }
        [Client]
        private void UpdateTurnUI(string turnOwnerId, int turnTime)
        {
            //Set Turn
            string currentPlayerId = AuthenticationService.Instance.PlayerId;
            cardBattleBoardPresenters.turnTimer.StartTimerBySeconds(turnTime);
            bool isCurrentPlayerOwner = currentPlayerId == turnOwnerId;
            if (isCurrentPlayerOwner)
            {
                onYourTurnStarted?.Invoke();
                cardBattleBoardPresenters.playerStateUI.SetParameter("Your turn");
                cardBattleBoardPresenters.enemyStateUI.SetParameter("Waiting for you");
                return;
            }
            onEnemyTurnStarted?.Invoke();
            cardBattleBoardPresenters.playerStateUI.SetParameter("Enemy turn");
            cardBattleBoardPresenters.enemyStateUI.SetParameter("Waiting for him");
        }

        #endregion
        
        [Client]
        public void DisconnectFromMatch()
        {
            NetworkManager.singleton.StopClient();
        }
    }

    #region Battle Data
    
    [Serializable]
    public class TableInfo
    {
        public string playerId;
        public string playerName;
        public MatchesStatistics matchesStatistics;
        public int points;
        public int turnCounter;
        public int currentFuel;
        public int maxFuel;
        public SlotInfo qgSlotInfo;
        public List<CardData> deckCardDatas;
        public List<CardData> handCardDatas;
        public List<CardData> cemeteryCardDatas;
        public List<BattleSlotInfo> slotInfos;

        public TableInfo(
            string playerId,
            string playerName,
            MatchesStatistics matchesStatistics,
            int currentFuel,
            int maxFuel,
            SlotInfo qgSlotInfo
            )
        {
            this.playerId = playerId;
            this.playerName = playerName;
            this.matchesStatistics = matchesStatistics;
            points = 0;
            turnCounter = 0;
            this.currentFuel = currentFuel;
            this.maxFuel = maxFuel;
            this.qgSlotInfo = qgSlotInfo;
            deckCardDatas = new List<CardData>();
            handCardDatas = new List<CardData>();
            cemeteryCardDatas = new List<CardData>();
            slotInfos = new List<BattleSlotInfo>();
        }

        public static NetworkedTableInfo[] TableInfosToStruct(List<TableInfo> tableInfosToConvert)
        {
            NetworkedTableInfo[] networkedTableInfos = new NetworkedTableInfo[tableInfosToConvert.Count];

            for (var index = 0; index < tableInfosToConvert.Count; index++)
            {
                var tableInfo = tableInfosToConvert[index];
                networkedTableInfos.SetValue(tableInfo.ToStruct(), index);
            }

            return networkedTableInfos;
        }
        
        public NetworkedTableInfo ToStruct()
        {
            NetworkedCardData[] NetworkedCardDatas(List<CardData> cardDatas)
            {
                NetworkedCardData[] networkedCardDatas = new NetworkedCardData[cardDatas.Count];

                for (var index = 0; index < cardDatas.Count; index++)
                {
                    var cardData = cardDatas[index];
                    NetworkedCardData networkedCardData = cardData.ToStruct();
                    networkedCardDatas.SetValue(networkedCardData, index);
                }

                return networkedCardDatas;
            }

            NetworkedSlotInfo qgNetworkedSlotInfo = qgSlotInfo.ToStruct();
                
            var networkedDeckCardDatas = NetworkedCardDatas(deckCardDatas);
            var networkedHandCardDatas = NetworkedCardDatas(handCardDatas);
            var networkedCemeteryCardDatas = NetworkedCardDatas(cemeteryCardDatas);
            
            var networkedSlotInfos = new NetworkedBattleSlotInfo[slotInfos.Count];

            for (var index = 0; index < slotInfos.Count; index++)
            {
                var slotInfo = slotInfos[index];
                NetworkedBattleSlotInfo networkedSlotInfo = slotInfo.ToStruct();
                networkedSlotInfos.SetValue(networkedSlotInfo, index);
            }

            return new NetworkedTableInfo
            {
                playerId = playerId,
                playerName = playerName,
                matchesStatistics = matchesStatistics,
                points = points,
                turnCounter = turnCounter,
                currentFuel = currentFuel,
                maxFuel = maxFuel,
                qgSlotInfo = qgNetworkedSlotInfo,
                deckCardDatas = networkedDeckCardDatas,
                handCardDatas = networkedHandCardDatas,
                cemeteryCardDatas = networkedCemeteryCardDatas,
                slotInfos = networkedSlotInfos
            };
        }
    }
    
    [Serializable]
    public class SlotInfo
    {
        public int id;
        public CardData slotCardData;
        public SlotData slotData;
        
        public NetworkedSlotInfo ToStruct()
        {
            return new NetworkedSlotInfo
            {
                id = id,
                slotData = slotData.ToStruct(),
                slotCardData = slotCardData != null ? slotCardData.ToStruct() : default
            };
        }
    }
    [Serializable]
    public class BattleSlotInfo : SlotInfo
    {
        public CardData slotSupportCardData;
        
        public new NetworkedBattleSlotInfo ToStruct()
        {
            return new NetworkedBattleSlotInfo
            {
                id = id,
                slotData = slotData.ToStruct(),
                slotCardData = slotCardData != null ? slotCardData.ToStruct() : default,
                slotSupportCardData = slotSupportCardData != null ? slotSupportCardData.ToStruct() : default
            };
        }
    }
    
    [Serializable]
    public struct NetworkedTableInfo
    {
        public string playerId;
        public string playerName;
        public MatchesStatistics matchesStatistics;
        public int points;
        public int turnCounter;
        public int currentFuel;
        public int maxFuel;
        public NetworkedSlotInfo qgSlotInfo;
        public NetworkedCardData[] deckCardDatas;
        public NetworkedCardData[] handCardDatas;
        public NetworkedCardData[] cemeteryCardDatas;
        public NetworkedBattleSlotInfo[] slotInfos;
    }
    [Serializable]
    public struct NetworkedSlotInfo
    {
        public int id;
        public NetworkedCardData slotCardData;
        public NetworkedSlotData slotData;
    }
    
    [Serializable]
    public struct NetworkedBattleSlotInfo
    {
        public int id;
        public NetworkedCardData slotCardData;
        public NetworkedCardData slotSupportCardData;
        public NetworkedSlotData slotData;
    }

    [Serializable]
    public struct NetworkedSlotData
    {
        public string[] types;
        public string bgImage;
    }
    
    [Serializable]
    public struct NetworkedCardData
    {
        public string cardName;
        public string cardEconomyId;
        public string cardInventoryId;
        public string effectDescription;
        public string requirement;
        public int attack;
        public int cost;
        public int health;
        public int maxDeckable;
        public string rarity;
        public string[] targetType;
        public string type;
    }
    
    #endregion
    [Serializable]
    public struct LeaderboardMetaData
    {
        public MatchesStatistics matchesStatistics;

        public LeaderboardMetaData(int matchesDraw, int matchesWon, int matchesLost)
        {
            matchesStatistics = new MatchesStatistics(matchesDraw, matchesWon, matchesLost);
        }
    }
    [Serializable]
    public struct MatchesStatistics
    {
        public int matchesDraw;
        public int matchesWon;
        public int matchesLost;
            
        public MatchesStatistics(int matchesDraw, int matchesWon, int matchesLost)
        {
            this.matchesWon = matchesWon;
            this.matchesLost = matchesLost;
            this.matchesDraw = matchesDraw;
        }
    }
}
