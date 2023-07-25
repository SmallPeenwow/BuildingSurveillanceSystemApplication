using System.Linq;

namespace BuildingSurveillanceSystemApplication;

class Program
{
    static void Main(string[] args)
    {

    }
}

public class SecuritySurveillanceHub : IObservable<ExternalVisitor>
{
    private List<ExternalVisitor> _externalVisitors;
    private List<IObserver<ExternalVisitor>> _observers;

    public IDisposable Subscribe(IObserver<ExternalVisitor> observer)
    {
        throw new NotImplementedException();
    }

    public void ConfirmExternalVisitorEntersBuilding(
        int id,
        string firstName,
        string lastName,
        string companyName,
        string jobTitle,
        DateTime entryDateTime,
        int employeeContactId)
    {
        ExternalVisitor externalVisitor = new ExternalVisitor
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            CompanyName = companyName,
            JobTitle = jobTitle,
            EntryDateTime = entryDateTime,
            InBuilding = true,
            EmployeeContactId = employeeContactId
        };

        _externalVisitors.Add(externalVisitor);

        // TODO: IObserver<ExternalVisitor> test with that
        foreach (var observer in _observers)
        {
            observer.OnNext(externalVisitor);
        }
    }

    public void ConfirmExternalVisitorExitsBuilding(int externalVisitorId, DateTime exitDateTime)
    {
        ExternalVisitor externalVisitor = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitorId);

        if (externalVisitor != null)
        {
            externalVisitor.ExitDateTime = exitDateTime;
            externalVisitor.InBuilding = false;

            foreach (IObserver<ExternalVisitor> observer in _observers)
            {
                observer.OnNext(externalVisitor);
            }
        }
    }

    public void BuildingEntryCutOffTimeReached()
    {
        if (_externalVisitors.Where(e => e.InBuilding == true).ToList().Count == 0)
        {
            foreach (IObserver<ExternalVisitor> observer in _observers)
            {
                observer.OnCompleted();
            }
        }
    }
}

public class ExternalVisitor
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CompanyName { get; set; }
    public string JobTitle { get; set; }
    public DateTime EntryDateTime { get; set; }
    public DateTime ExitDateTime { get; set; }
    public bool InBuilding { get; set; }
    public int EmployeeContactId { get; set; }
}