namespace GenSync.ProgressReport
{
    public interface IGeneralProgressUi
    {
        void SetProgressValue(int percent);

        void Close();
    }
}
