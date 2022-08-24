namespace Data.Piers
{
    // all internal classes should implements _i_ICommand
    // this interface is for external use only
    public interface ICommand
    {
        IResponse Execute();
    }
    
}
