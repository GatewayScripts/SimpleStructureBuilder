using Autofac;
using Prism.Events;
using StructureBuilder.ViewModels;
using StructureBuilder.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace StructureBuilder.Startup
{
    public class Bootstrapper
    {
        public IContainer Boostrap(StructureSet ss, Application application)
        {
            var container = new ContainerBuilder();
            //misc
            container.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            //esapi
            container.RegisterInstance(ss);
            container.RegisterInstance(application);
            //view
            container.RegisterType<MainView>().AsSelf();
            //viewModels
            container.RegisterType<MainViewModel>().AsSelf();
            return container.Build();
        }
    }
}
