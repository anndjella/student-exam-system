namespace Application.Validators.Person
{
    public interface IPersonCreate
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string JMBG { get; set; }
    }
}
