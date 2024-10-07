using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace Game
{
    public class CardInfoPresenter : MonoBehaviour
    {
        [SerializeField] private SO_ID item_ID, Sprite_ID;
        [SerializeField] private ItemsList cardTemplates, typeTemplates;
        [SerializeField] private Transform targetsParent;
        [SerializeField] private Transform statsParent;
        [SerializeField] private Image targetPrefab;
        [SerializeField] private CardInfoStatsPresenter healthStatsPrefab;
        [SerializeField] private CardInfoStatsPresenter attackStatsPrefab;
        [SerializeField] private CardInfoStatsPresenter fuelStatsPrefab;
        [SerializeField] private TextMeshProUGUI cardNameText, rarityText, typeText, descriptionText;
        [SerializeField] private Image cardImage;
        
        public void InizializeCardInfo(object objectParameter)
        {
            BaseCard baseCard = (BaseCard)objectParameter;
            //Set battleCard background
            string cardEconomyId = baseCard.GetComponent<CardEconomyIdComponent>().Parameter;
            BasePropertiesItem cardTemplate = cardTemplates.items.Find(x => x.GetItemValue<string>(item_ID.id) == cardEconomyId);
            Sprite sprite = cardTemplate.GetItemValue<Sprite>(Sprite_ID.id);
            cardImage.sprite = sprite;
            //Set battleCard name
            string cardName = baseCard.GetComponent<CardNameComponent>().Parameter;
            cardNameText.text = cardName;
            //Set battleCard description
            string rarity = baseCard.GetComponent<RarityComponent>().Parameter;
            rarityText.text = CapitalizeFirstLetter(rarity);
            //Set battleCard type
            string cardType = baseCard.GetComponent<CardTypeComponent>().Parameter;
            typeText.text = CapitalizeFirstLetter(cardType);
            //Set battleCard targetTypes
            if (baseCard.TryGetComponent(out TargetTypesComponent targetTypesComponent))
            {
                foreach (var type in targetTypesComponent.Parameter)
                {
                    Image target = Instantiate(targetPrefab, targetsParent);
                    BasePropertiesItem typeTemplate = typeTemplates.items.Find(x => x.GetItemValue<string>(item_ID.id) == type);
                    Sprite typeSprite = typeTemplate.GetItemValue<Sprite>(Sprite_ID.id);
                    target.sprite = typeSprite;
                }
            }
            //Set battleCard health
            if (baseCard.TryGetComponent(out HealthComponent healthComponent))
            {
                CardInfoStatsPresenter healthStats = Instantiate(healthStatsPrefab, statsParent);
                healthStats.SetStats(healthComponent.Parameter.ToString());
            }
            //Set battleCard fuel
            if (baseCard.TryGetComponent(out CostComponent costComponent))
            {
                CardInfoStatsPresenter costStats = Instantiate(fuelStatsPrefab, statsParent);
                costStats.SetStats(costComponent.Parameter.ToString());
            }
            //Set battleCard attack
            if (baseCard.TryGetComponent(out AttackComponent attackComponent))
            {
                CardInfoStatsPresenter attackStats = Instantiate(attackStatsPrefab, statsParent);
                attackStats.SetStats(attackComponent.Parameter.ToString());
            }
            //Set battleCard description
            string effectDescription = baseCard.GetComponent<EffectDescriptionComponent>().Parameter;
            descriptionText.text = effectDescription;
        }

        public static string CapitalizeFirstLetter(string input)
        {
            return  char.ToUpper(input[0]) + input[1..];
        }
    }
}
