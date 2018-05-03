namespace SQLCompare.Core.Interfaces.Services
{
    public interface IProjectService
    {
        void SaveProject();

        void CloseProject();

        void LoadProject(string filename);

        void Compare();
    }
}