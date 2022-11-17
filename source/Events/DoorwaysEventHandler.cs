using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Events
{
    public delegate void DoorwaysEventHandler<T>(T data);
}
