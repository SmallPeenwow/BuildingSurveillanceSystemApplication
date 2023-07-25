﻿using System.Linq;

namespace BuildingSurveillanceSystemApplication;

class Program
{
    static void Main(string[] args)
    {

    }
}

public class Employee : IEmployee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string JobTitle { get; set; }
}

public interface IEmployee
{
    int Id { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
    string JobTitle { get; set; }
}

public abstract class Observer : IObserver<ExternalVisitor>
{
    IDisposable _cancellation;
    protected List<ExternalVisitor> _externalVisitors = new List<ExternalVisitor>();

    public abstract void OnCompleted();

    public abstract void OnError(Exception error);

    public abstract void OnNext(ExternalVisitor value);

    public void Subscribe(IObservable<ExternalVisitor> provider)
    {
        _cancellation = provider.Subscribe(this);
    }

    public void UnSubscribe()
    {
        _cancellation.Dispose();
        _externalVisitors.Clear();
    }
}

public class EmployeeNotify : Observer
{
    IEmployee _employee = null;
    

    public EmployeeNotify(IEmployee employee)
    {
        _employee = employee;
    }

    public override void OnCompleted()
    {
        string heading = $"{_employee.FirstName + " " + _employee.LastName} Daily Visitors's Report";
        Console.WriteLine();

        Console.WriteLine(heading);
        Console.WriteLine(new string('-', heading.Length));
        Console.WriteLine();

        foreach (ExternalVisitor externalVisitor in _externalVisitors)
        {
            externalVisitor.InBuilding = false;

            Console.WriteLine($"{externalVisitor.Id, -6} {externalVisitor.FirstName, -15}{externalVisitor.LastName, -15}{externalVisitor.EntryDateTime.ToString("dd MMM yyyy hh:mm:ss"), -25}{externalVisitor.ExitDateTime.ToString("dd MMM yyyy hh:mm:ss"), -25}");
        }

        Console.WriteLine();
        Console.WriteLine();
    }

    public override void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public override void OnNext(ExternalVisitor value)
    {
        ExternalVisitor externalVisitor = value;

        if (externalVisitor.EmployeeContactId == _employee.Id)
        {
            ExternalVisitor externalVisistorListItem = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitor.Id);

            if (externalVisistorListItem == null)
            {
                _externalVisitors.Add(externalVisitor);

                Console.WriteLine($"{_employee.FirstName}" +
                    $" {_employee.LastName}," +
                    $" your visitor has arrived. Visitor Id({externalVisitor.Id})," +
                    $" FirstName({externalVisitor.FirstName}), " +
                    $"LastName({externalVisitor.LastName})," +
                    $" entered the builing, DateTime({externalVisitor.EntryDateTime.ToString("dd MMM yyyy hh:mm:ss")})");
                Console.WriteLine();
            }
            else
            {
                if (externalVisitor.InBuilding == false)
                {
                    externalVisistorListItem.InBuilding = false;
                    externalVisistorListItem.ExitDateTime = externalVisitor.ExitDateTime;
                }
            }
        }
    }   
}

public class SecurityNotify : Observer
{
    public override void OnCompleted()
    {
        string heading = "Security Daily Visitor's Report";
        Console.WriteLine();

        Console.WriteLine(heading);
        Console.WriteLine(new string('-', heading.Length));
        Console.WriteLine();

        foreach (ExternalVisitor externalVisitor in _externalVisitors)
        {
            externalVisitor.InBuilding = false;

            Console.WriteLine($"{externalVisitor.Id,-6}{externalVisitor.FirstName,-15}{externalVisitor.LastName,-15}{externalVisitor.EntryDateTime.ToString("dd MMM yyyy hh:mm:ss"),-25}{externalVisitor.ExitDateTime.ToString("dd MMM yyyy hh:mm:ss tt"),-25}");
        }

        Console.WriteLine();
        Console.WriteLine();
    }

    public override void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public override void OnNext(ExternalVisitor value)
    {
        ExternalVisitor externalVisitor = value;

        ExternalVisitor externalVisitorListItem = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitor.Id);

        if (externalVisitorListItem == null)
        {
            _externalVisitors.Add(externalVisitor);

            //OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Security);

            Console.WriteLine($"Security notification: Visitor Id({externalVisitor.Id}), FirstName({externalVisitor.FirstName}), LastName({externalVisitor.LastName}), entered the building, DateTime({externalVisitor.EntryDateTime.ToString("dd MMM yyyy hh:mm:ss tt")})");

            //OutputFormatter.ChangeOutputTheme(OutputFormatter.TextOutputTheme.Normal);

            Console.WriteLine();
        }
        else
        {
            if (externalVisitor.InBuilding == false)
            {
                //update local external visitor list item with data from the external visitor object passed in from the observable object
                externalVisitorListItem.InBuilding = false;
                externalVisitorListItem.ExitDateTime = externalVisitor.ExitDateTime;

                Console.WriteLine($"Security notification: Visitor Id({externalVisitor.Id}), FirstName({externalVisitor.FirstName}), LastName({externalVisitor.LastName}), exited the building, DateTime({externalVisitor.ExitDateTime.ToString("dd MMM yyyy hh:mm:ss tt")})");
                Console.WriteLine();
            }
        }
    }
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