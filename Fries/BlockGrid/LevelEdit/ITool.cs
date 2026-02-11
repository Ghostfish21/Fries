namespace Fries.BlockGrid.LevelEdit {
    public interface ITool {
        int GetId();
        void OnLMBClicked(BlockKey blockKey);
        void OnRMBClicked(BlockKey blockKey);
        void OnMBBClicked(BlockKey blockKey);
    }
}