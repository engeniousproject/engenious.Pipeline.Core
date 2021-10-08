namespace engenious.Content.Models.History
{
    public interface IHistoryItem
    {
        void Undo();
        void Redo();
    }
}