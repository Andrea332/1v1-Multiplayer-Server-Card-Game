using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;
using Utilities;

namespace Game
{
    [CreateAssetMenu(fileName = "BaseCardAttack", menuName = "Cards/Attack/Base Card Attack")]
    public class BaseCardAttack : ScriptableObject
    {
        public BaseItemString submarineItemId;
        
        public virtual void ServerTryAttackCard(CardBattleBoardManager cardBattleBoardManager, int receiverSlotId, string receiverSlotOwnerId, string attackerCardInventoryId, string attackerPlayerId)
        {
            TableInfo receiverTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId == receiverSlotOwnerId);
            
            TableInfo attackerTableInfo = cardBattleBoardManager.tableInfos.Find(x => x.playerId != receiverSlotOwnerId);

            SlotInfo receiverSlotInfo = receiverTableInfo.slotInfos.Find(x => x.id == receiverSlotId);
            
            SlotInfo attackerSlotInfo = attackerTableInfo.slotInfos.Find(x => x.slotCardData != null && x.slotCardData.cardInventoryId == attackerCardInventoryId);

            //Check if the selected slot is owned by this player, if not deny the action
            if (ReceiverSlotAndAttackerCardOwnerIdIsEqual(cardBattleBoardManager, receiverSlotId, receiverSlotOwnerId, attackerCardInventoryId, attackerPlayerId)) return;
            //Check if the receiver slot has a card on it, if not deny the action
            if (SlotDoesntHaveCardOnIt(cardBattleBoardManager, receiverSlotInfo)) return;
            //Check if the slot already attacked, if it has deny action
            if (HasSlotAlreadyAttacked(cardBattleBoardManager, attackerSlotInfo)) return;
            //Check if at least one target types of the attacker card is equal to the receiver card's type, if not deny the action
            if (AttackerCardTargetIsReceiverCardType(cardBattleBoardManager, attackerSlotInfo, receiverSlotInfo)) return;
            //Check if card is attacking a smokescreen slot
            if(SmokeScreenCheck(cardBattleBoardManager, receiverSlotInfo, attackerTableInfo)) return;
            //Check if card has a ServerDiveAbility and if the attacker's card is of submarine type, if not deny the action
            if (SlotHasDiveAbilityOn(cardBattleBoardManager, receiverSlotInfo, attackerSlotInfo)) return;
      
            ServerAttackCard(cardBattleBoardManager, receiverSlotOwnerId, attackerSlotInfo, receiverSlotInfo, attackerTableInfo, receiverTableInfo);
        }
        private bool SlotDoesntHaveCardOnIt(CardBattleBoardManager cardBattleBoardManager, SlotInfo receiverSlotInfo)
        {
            if (receiverSlotInfo.slotCardData != null) return false;
            
            string message = $"Slot {receiverSlotInfo.id} has no battleCard on it";
            Debug.Log(message);
            cardBattleBoardManager.PingToClient(message);
            return true;
        }

        private bool ReceiverSlotAndAttackerCardOwnerIdIsEqual(CardBattleBoardManager cardBattleBoardManager, int attackedSlotId, string attackedSlotOwnerId, string attackedCardInventoryId, string attackerCardOwnerId)
        {
            if (attackedSlotOwnerId != attackerCardOwnerId) return false;
            
            string message = $"Player {attackerCardOwnerId} can not attack battleCard {attackedCardInventoryId} on slot {attackedSlotId} of player {attackedSlotOwnerId}";
            Debug.Log(message);
            cardBattleBoardManager.PingToClient(message);
            return true;
        }

        private bool HasSlotAlreadyAttacked(CardBattleBoardManager cardBattleBoardManager, SlotInfo attackerSlotInfo)
        {
            if (!attackerSlotInfo.slotCardData.AttackDone) return false;
            
            string message = "Slot battleCard already attacked";
            Debug.Log(message);
            cardBattleBoardManager.PingToClient(message);
            return true;
        }

        private protected virtual bool AttackerCardTargetIsReceiverCardType(CardBattleBoardManager cardBattleBoardManager, SlotInfo attackerSlotInfo, SlotInfo receiverSlotInfo)
        {
            if (attackerSlotInfo.slotCardData.targetType.Find(x => x == receiverSlotInfo.slotCardData.type) != null) return false;
            
            string message = "BattleCard that is attacking can not attack a battleCard that has a type different from target types";
            Debug.Log(message);
            cardBattleBoardManager.PingToClient(message);
            return true;
        }
        
        private bool SlotHasDiveAbilityOn(CardBattleBoardManager cardBattleBoardManager, SlotInfo receiverSlotInfo, SlotInfo attackerSlotInfo)
        {
            if (receiverSlotInfo.slotCardData.optionalParameter is not ServerDiveAbility || attackerSlotInfo.slotCardData.type == submarineItemId.ItemValue) return false;
            
            string message = "BattleCard that is attacking is not a submarine";
            Debug.Log(message);
            cardBattleBoardManager.PingToClient(message);
            return true;
        }
        
        private protected virtual void ServerAttackCard(CardBattleBoardManager cardBattleBoardManager,  string receiverSlotOwnerId, SlotInfo attackerSlotInfo, SlotInfo receiverSlotInfo, TableInfo attackerTableInfo, TableInfo receiverTableInfo)
        {
            attackerSlotInfo.slotCardData.currentAttackTimes++;
            receiverSlotInfo.slotCardData.DamageCard(attackerSlotInfo.slotCardData.attack);
            if (receiverSlotInfo.slotCardData.health <= 0)
            {
                if (receiverSlotInfo.slotCardData.optionalParameter is IAbilityAble abilityManager)
                {
                    abilityManager.RemoveAbilityUsed();
                }
                receiverSlotInfo.slotCardData.ResetToDefaultValues();
                attackerTableInfo.points += receiverSlotInfo.slotCardData.cost;
                receiverTableInfo.cemeteryCardDatas.Add(receiverSlotInfo.slotCardData);
                receiverSlotInfo.slotCardData = null;
            }
            
            cardBattleBoardManager.ClientRpcAttackSlot(receiverSlotInfo.id, receiverSlotOwnerId, attackerSlotInfo.slotCardData.cardEconomyId, attackerSlotInfo.slotCardData.attack);
            cardBattleBoardManager.ClientRpcUpdateUI(TableInfo.TableInfosToStruct(cardBattleBoardManager.tableInfos));
        }
     
        public virtual void ClientAttackCard(CardBattleBoardManager cardBattleBoardManager, int receiverSlotId, string receiverSlotOwnerId, int damage)
        {
            TableSlot tableSlot;
            if (receiverSlotOwnerId == AuthenticationService.Instance.PlayerId)
            {
                tableSlot = cardBattleBoardManager.playerTableSlots.Find(x => x.Id == receiverSlotId);
            }
            else
            {
                tableSlot = cardBattleBoardManager.enemyTableSlots.Find(x => x.Id == receiverSlotId);
            }
            
            if(tableSlot.CurrentBattleCard == null) return;
            
            if (!tableSlot.CurrentBattleCard.TryGetComponent(out HealthComponent healthComponent)) return;
            
            healthComponent.Parameter -= damage;
            
            if (healthComponent.Parameter > 0) return;
            
            healthComponent.ResetParameter();
            
            if(tableSlot.CurrentBattleCard.TryGetComponent(out IAbilityAble abilityAble))
            {
                abilityAble.RemoveAbilityUsed();
            }
            
            if (tableSlot.PlayerOwnerId == AuthenticationService.Instance.PlayerId)
            {
                cardBattleBoardManager.playerCards.Remove(tableSlot.CurrentBattleCard);
                tableSlot.CurrentBattleCard.transform.SetParent(cardBattleBoardManager.playerCemeteryCardPosition, false);
                cardBattleBoardManager.cemeteryCards.Add(tableSlot.CurrentBattleCard);
            }
            else
            {
                cardBattleBoardManager.enemyCards.Remove(tableSlot.CurrentBattleCard);
                Destroy(tableSlot.CurrentBattleCard.gameObject);
            }
           
            tableSlot.UnSetCard();
        }
        
        public bool MineCheck(CardBattleBoardManager cardBattleBoardManager, SlotInfo attackerSlotInfo, TableInfo attackerTableInfo, TableInfo receiverTableInfo)
        {
            List<BattleSlotInfo> minesSlotInfo = receiverTableInfo.slotInfos.FindAll(x => x.slotCardData?.optionalParameter is ServerMineAbility);

            BattleSlotInfo mineSlotToUse = minesSlotInfo.Find(x =>
            {
                if (x.slotCardData.optionalParameter is ServerMineAbility serverMineAbility)
                {
                    if (serverMineAbility.MineTarget.ItemValue == attackerSlotInfo.slotCardData.type)
                    {
                        return true;
                    }
                }

                return false;
            });
            
            //Mine not found do nothing
            if (mineSlotToUse == null) return false;
       
            //Mine found 
            //Destroy Mine
            var serverMineAbility = (ServerMineAbility)mineSlotToUse.slotCardData.optionalParameter;
            serverMineAbility.RemoveAbilityUsed();
            mineSlotToUse.slotCardData.ResetToDefaultValues();
            receiverTableInfo.cemeteryCardDatas.Add(mineSlotToUse.slotCardData);
            mineSlotToUse.slotCardData = null;
            cardBattleBoardManager.ClientRpcDestroySlotMainCard(mineSlotToUse.id, receiverTableInfo.playerId);
            //Cause attacker card damage
            attackerSlotInfo.slotCardData.DamageCard(serverMineAbility.MineDamage);
            cardBattleBoardManager.ClientRpcAttackSlot(attackerSlotInfo.id, attackerTableInfo.playerId, attackerSlotInfo.slotCardData.cardEconomyId, serverMineAbility.MineDamage);
            if (attackerSlotInfo.slotCardData.health <= 0)
            {
                if (attackerSlotInfo.slotCardData.optionalParameter is IAbilityAble abilityManager)
                {
                    abilityManager.RemoveAbilityUsed();
                }
                attackerSlotInfo.slotCardData.ResetToDefaultValues();
                receiverTableInfo.points += attackerSlotInfo.slotCardData.cost;
                attackerTableInfo.cemeteryCardDatas.Add(attackerSlotInfo.slotCardData);
                attackerSlotInfo.slotCardData = null;
            }
            else
            {
                //If attacked card survived attack receiver slot
                return false;
            }
            cardBattleBoardManager.ClientRpcUpdateUI(TableInfo.TableInfosToStruct(cardBattleBoardManager.tableInfos));
            return true;
        }
        
        public bool BunkerCheck(CardBattleBoardManager cardBattleBoardManager, SlotInfo attackerSlotInfo, SlotInfo receiverSlotInfo, TableInfo attackerTableInfo, TableInfo receiverTableInfo)
        {
            List<BattleSlotInfo> bunkersSlotInfo = receiverTableInfo.slotInfos.FindAll(x => x.slotCardData?.optionalParameter is ServerBunkerAbility);

            BattleSlotInfo bunkerSlotToUse = bunkersSlotInfo.Find(x =>
            {
                if (x.slotCardData.optionalParameter is ServerBunkerAbility ability)
                {
                    if (ability.CardDataToProtect.cardInventoryId == receiverSlotInfo.slotCardData.cardInventoryId)
                    {
                        return true;
                    }
                }

                return false;
            });

            if (bunkerSlotToUse == null) return false;
            
            attackerSlotInfo.slotCardData.currentAttackTimes++;
            bunkerSlotToUse.slotCardData.DamageCard(attackerSlotInfo.slotCardData.attack);
            if (bunkerSlotToUse.slotCardData.health <= 0)
            {
                if (bunkerSlotToUse.slotCardData.optionalParameter is IAbilityAble abilityManager)
                {
                    abilityManager.RemoveAbilityUsed();
                }
                bunkerSlotToUse.slotCardData.ResetToDefaultValues();
                attackerTableInfo.points += bunkerSlotToUse.slotCardData.cost;
                receiverTableInfo.cemeteryCardDatas.Add(bunkerSlotToUse.slotCardData);
                bunkerSlotToUse.slotCardData = null;
            }
            
            cardBattleBoardManager.ClientRpcAttackSlot(bunkerSlotToUse.id, receiverTableInfo.playerId, attackerSlotInfo.slotCardData.cardEconomyId, attackerSlotInfo.slotCardData.attack);
            cardBattleBoardManager.ClientRpcUpdateUI(TableInfo.TableInfosToStruct(cardBattleBoardManager.tableInfos));
            return true;
        }
        
        public bool SmokeScreenCheck(CardBattleBoardManager cardBattleBoardManager, SlotInfo receiverSlotInfo, TableInfo attackerTableInfo)
        {
            List<BattleSlotInfo> smokescreenSlotInfos = attackerTableInfo.slotInfos.FindAll(slotInfo =>
            {
                if (slotInfo.slotCardData == null) return false;
                return slotInfo.slotCardData.optionalParameter is ServerSmokescreenAbility;
            });
            
            BattleSlotInfo smokescreenSlotToUse = smokescreenSlotInfos.Find(smokescreenSlotInfo =>
            {
                var ability = (ServerSmokescreenAbility)smokescreenSlotInfo.slotCardData.optionalParameter;

                if (ability.SlotInfoToSmokescreen.slotCardData == null) return false;
                
                return ability.SlotInfoToSmokescreen.slotCardData.cardInventoryId == receiverSlotInfo.slotCardData.cardInventoryId;
            });

            if (smokescreenSlotToUse == null) return false;
            
            string message = "Slot battleCard has smokeScreen on it";
            Debug.Log(message);
            cardBattleBoardManager.PingToClient(message);
            return true;
        }
    }
}
