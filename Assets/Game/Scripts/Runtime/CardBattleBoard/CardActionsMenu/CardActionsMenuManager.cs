using System;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class CardActionsMenuManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        public CardBattleBoardManager cardBattleBoardManager;
        public Transform battleCardParent;
        public Transform supportBattleCardParent;
        [HideInInspector] public BattleCard battleCard;
        [HideInInspector] public BattleCard supportBattleCard;
        [SerializeField] private Button battleCardAbilityButton;
        [SerializeField] private Button supportCardAbilityButton;
        [SerializeField] private SO_ID item_UGS_Id;
        [SerializeField] private SO_ID item_Ability_Id;
        [SerializeField] private ItemsList cardTemplates;

        private void Start()
        {
            CloseMenu();
        }
        
        public void SetMenu(CardBattleBoardManager cardBattleBoardManager, TableSlot tableSlot, BaseCard baseCard)
        {
            if(baseCard == null) return;
            
            DestroyOldMenu();
            OpenMenu();
            battleCard = tableSlot.CurrentBattleCard;
            if (tableSlot is TableBattleSlot tableBattleSlot)
            {
                supportBattleCard = tableBattleSlot.CurrentSupportCard;
            }
            else
            {
                supportBattleCard = null;
            }
            CreateCardsCopy();
            SetAbilitiesButtons();
        }

        public void OpenMenu()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        public void CloseMenu()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private bool HasCardAbility(string cardEconomyId)
        {
            BasePropertiesItem cardItem = cardTemplates.FindPropertiesItem(item_UGS_Id.id, cardEconomyId);

            BaseCardAbility baseCardAbility = cardItem.GetItem<BaseCardAbility>(item_Ability_Id.id);
            
            return baseCardAbility != null;
        }
        
        private bool HasAbilityDone(BattleCard battleCard)
        {
            return battleCard.TryGetComponent(out IAbilityAble ability) && ability.AbilityUsed;
        }

        private void SetAbilitiesButtons()
        {
            string battleCardEconomyId = string.Empty;
            bool battleCardNotNull = battleCard != null;
            if (battleCardNotNull)
            {
                battleCardEconomyId = battleCard.GetComponent<CardEconomyIdComponent>().Parameter;
            }

            string supportCardEconomyId = string.Empty;
            bool supportCardNotNull = supportBattleCard != null;
            if (supportCardNotNull)
            {
                supportCardEconomyId = supportBattleCard.GetComponent<CardEconomyIdComponent>().Parameter;
            }
            
            battleCardAbilityButton.interactable = battleCardNotNull && HasCardAbility(battleCardEconomyId) && !HasAbilityDone(battleCard);
            supportCardAbilityButton.interactable = supportCardNotNull && HasCardAbility(supportCardEconomyId) && !HasAbilityDone(supportBattleCard);
        }

        public void DoBattleCardAbility()
        {
            string cardInventoryId = battleCard.GetComponent<CardInventoryIdComponent>().Parameter;
            string cardEconomyId = battleCard.GetComponent<CardEconomyIdComponent>().Parameter;
            cardBattleBoardManager.PrepareAbility(cardEconomyId, cardInventoryId);
            CloseMenu();
        }

        public void DoSupportAbility()
        {
            string cardInventoryId = supportBattleCard.GetComponent<CardInventoryIdComponent>().Parameter;
            string cardEconomyId = supportBattleCard.GetComponent<CardEconomyIdComponent>().Parameter;
            cardBattleBoardManager.PrepareAbility(cardEconomyId, cardInventoryId);
            CloseMenu();
        }

        private void CreateCardsCopy()
        {
            BattleCard tempBattleCard = Instantiate(battleCard, battleCardParent, false);
            tempBattleCard.canvasGroup.interactable = false;
            tempBattleCard.canvasGroup.blocksRaycasts = false;
            
            if(supportBattleCard == null) return;
            
            BattleCard tempSupportCard = Instantiate(supportBattleCard, supportBattleCardParent, false);
            tempSupportCard.gameObject.SetActive(true);
            tempSupportCard.canvasGroup.interactable = false;
            tempSupportCard.canvasGroup.blocksRaycasts = false;
            RectTransform rectTransform = (RectTransform)tempSupportCard.transform;
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        private void DestroyOldMenu()
        {
            if (battleCardParent.childCount != 0)
            {
                Destroy(battleCardParent.GetChild(0).gameObject);
            }
            
            if (supportBattleCardParent.childCount != 0)
            {
                Destroy(supportBattleCardParent.GetChild(0).gameObject);
            }
        }
    }
}
