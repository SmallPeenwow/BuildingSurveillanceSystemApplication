using System.Linq;

namespace BuildingSurveillanceSystemApplication;

class Program
{
    static void Main(string[] args)
    {

    }
}

public class EmployeeNotify : IObserver<ExternalVisitor>
{

}

public class UnSubscriber<ExternalVisitor> : IDisposable
{
    private List<IObserver<ExternalVisitor>> _observers;
    private IObserver<ExternalVisitor> _observer;

    public UnSubscriber(List<IObserver<ExternalVisitor>> observers, IObserver<ExternalVisitor> observer)
    {
        _observers = observers;
        _observer = observer;
    }

    public void Dispose()
    {
        if (_observers.Contains(_observer))
        {
            _observers.Remove(_observer);
        }
    }
}

public class SecuritySurveillanceHub : IObservable<ExternalVisitor>
{
    private List<ExternalVisitor> _externalVisitors;
    private List<IObserver<ExternalVisitor>> _observers;

    public IDisposable Subscribe(IObserver<ExternalVisitor> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }

        foreach (ExternalVisitor externalVisitor in _externalVisitors)
        {
            observer.OnNext(externalVisitor);
        }

        return new UnSubscriber<ExternalVisitor>(_observers, observer);
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

        foreach (IObserver<ExternalVisitor> observer in _observers)
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