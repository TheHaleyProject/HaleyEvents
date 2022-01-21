using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Enums;

namespace Haley.Abstractions
{
    public interface ISubscriber
    {
        string Id { get; set; }
        Type DeclaringType { get; }
        string GroupId { get; }
        string ListenerMethod { get; set; }
        void SendMessage(params object[] args);
    }
}
