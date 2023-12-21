using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skview_kepware_influx_service.MqttFiles
{
    public interface iMqttConect
    {
        public Task StartOperation();
    }
}
