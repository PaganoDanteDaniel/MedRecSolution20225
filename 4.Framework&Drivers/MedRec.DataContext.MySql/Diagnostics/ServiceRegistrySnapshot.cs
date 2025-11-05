using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace MedRec.DataContext.MySql.Diagnostics;

public sealed class ServiceRegistrySnapshot
{
    public IReadOnlyList<ServiceDescriptor> Descriptors { get; }

    public ServiceRegistrySnapshot(IEnumerable<ServiceDescriptor> services)
    {
        Descriptors = services.ToList().AsReadOnly();
    }

    public string ToText()
    {
        var sb = new StringBuilder();
        foreach (var d in Descriptors.OrderBy(x => x.ServiceType.FullName))
        {
            var impl = d.ImplementationType?.FullName
                       ?? (d.ImplementationInstance != null ? d.ImplementationInstance.GetType().FullName
                           : d.ImplementationFactory != null ? "<factory>" : "<unknown>");

            sb.AppendLine($"{d.Lifetime,-9} {d.ServiceType.FullName} -> {impl}");
        }
        return sb.ToString();
    }
}
