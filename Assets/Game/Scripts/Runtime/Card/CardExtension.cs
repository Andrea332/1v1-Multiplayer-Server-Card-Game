using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public static class CardExtension
    {
        public static void TrySetCardTypeComponent(this BaseCard card, string cardType, ParameterPresenter<string> presenterPrefab = null)
        {
            SetComponent<CardTypeComponent, string>(card.gameObject, cardType, presenterPrefab);
        }
        public static void TrySetRarityComponent(this BaseCard card, string rarity, ParameterPresenter<string> presenterPrefab = null)
        {
            SetComponent<RarityComponent, string>(card.gameObject, rarity, presenterPrefab);
        }
        public static void TrySetCardEconomyIdComponent(this BaseCard card, string cardEconomyId, ParameterPresenter<string> presenterPrefab = null)
        {
            SetComponent<CardEconomyIdComponent, string>(card.gameObject, cardEconomyId, presenterPrefab);
        }
        public static void TrySetInventoryIdComponent(this BaseCard card, string cardInventoryId, ParameterPresenter<string> presenter = null)
        {
            CardInventoryIdComponent cardInventoryIdComponent = card.gameObject.AddComponent<CardInventoryIdComponent>();
            cardInventoryIdComponent.onParameterChanged = new UnityEvent<string>();

            if (presenter != null)
            {
                cardInventoryIdComponent.onParameterChanged.AddListener(presenter.SetParameter);
            }
            
            cardInventoryIdComponent.Parameter = cardInventoryId;
        }
        public static void TrySetAttackComponent(this BaseCard card, int attack, ParameterPresenter<int> presenterPrefab = null)
        {
            if (attack == 0) return;
        
            SetComponent<AttackComponent, int>(card.gameObject, attack, presenterPrefab);
        }
    
        public static void TrySetCostComponent(this BaseCard card, int cost, ParameterPresenter<int> presenterPrefab = null)
        {
            if (cost == 0) return;
        
            SetComponent<CostComponent, int>(card.gameObject, cost, presenterPrefab);
        }
    
        public static void TrySetHealthComponent(this BaseCard card, int health, ParameterPresenter<int> presenterPrefab = null)
        {
            if (health == 0) return;
        
            SetComponent<HealthComponent, int>(card.gameObject, health, presenterPrefab);
        }
        public static void TrySetNameComponent(this BaseCard card, string name, ParameterPresenter<string> presenterPrefab = null)
        {
            SetComponent<CardNameComponent, string>(card.gameObject, name, presenterPrefab);
        }
        public static void TrySetCardImageComponent(this BaseCard card, string cardType, ParameterPresenter<string> presenter = null)
        {
            CardImageComponent cardImageComponent = card.gameObject.AddComponent<CardImageComponent>();
            cardImageComponent.onParameterChanged = new UnityEvent<string>();

            if (presenter != null)
            {
                cardImageComponent.onParameterChanged.AddListener(presenter.SetParameter);
            }
            
            cardImageComponent.Parameter = cardType;
        }
        public static void TrySetEffectDescriptionComponent(this BaseCard card, string effectDescription, ParameterPresenter<string> presenterPrefab = null)
        {
            SetComponent<EffectDescriptionComponent, string>(card.gameObject, effectDescription, presenterPrefab);
        }
        public static void TrySetMaxDeckableComponent(this BaseCard card, int maxDeckable, ParameterPresenter<int> presenterPrefab = null)
        {
            SetComponent<MaxDeckableComponent, int>(card.gameObject, maxDeckable, presenterPrefab);
        }
        
        
        public static void TrySetTargetTypesComponent(this BaseCard card, List<string> targetTypes, ParameterPresenter<List<string>> presenterPrefab = null)
        {
            if(targetTypes.Count == 0) return;
            
            SetComponent<TargetTypesComponent, List<string>>(card.gameObject, targetTypes, presenterPrefab);
        }
    
        private static void SetComponent<SpecificParameterComponent, T>(GameObject parentGameObject, T parameter, ParameterPresenter<T> presenterPrefab = null) where SpecificParameterComponent : ParameterComponent<T>
        {
            SpecificParameterComponent parameterComponent = parentGameObject.AddComponent<SpecificParameterComponent>();
            parameterComponent.onParameterChanged = new UnityEvent<T>();

            if (presenterPrefab != null)
            {
                Transform[] transforms = parentGameObject.GetComponentsInChildren<Transform>();
                Transform parameterPresenters = transforms.ToList().Find(x => x.gameObject.name == "ParameterPresenters");
                ParameterPresenter<T> parameterPresenter = Object.Instantiate(presenterPrefab, parameterPresenters.transform);
                parameterComponent.onParameterChanged.AddListener(parameterPresenter.SetParameter);
            }
            
            parameterComponent.Parameter = parameter;
            parameterComponent.DefaultParameter = parameter;
        }
        
    }
}