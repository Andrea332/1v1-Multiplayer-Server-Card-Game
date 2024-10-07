namespace Game
{
    public interface ICardInteractable
    {
        public void OnBeginDragWithCard(BaseCard card);
        
        public void OnEndDragWithCard(BaseCard card);
        
        public void EnterPointerOverCard(BaseCard card);
        
        public void ExitPointerOverCard(BaseCard card);

        public void RemoveCardFromCardInteraction(BaseCard card);

        public void AddCardFromCardInteraction(BaseCard card, ICardInteractable oldInteractAble);
        
    }
}
