﻿using System.Linq;
using System.Threading.Tasks;
using Dependencies.Check;
using Dependencies.Check.Models;
using Dependencies.Viewer.Wpf.Controls.Extensions;
using Dependencies.Viewer.Wpf.Controls.Models;
using Dependencies.Viewer.Wpf.Controls.Services;
using Dependencies.Viewer.Wpf.Controls.ViewModels;
using Dependencies.Viewer.Wpf.Controls.Views.Check;
using MaterialDesignThemes.Wpf;

namespace Dependencies.Viewer.Wpf.Controls.Commands
{
    public class CheckCommand
    {
        private readonly MainBusyService busyService;

        public CheckCommand(MainBusyService busyService)
        {
            this.busyService = busyService;
        }

        public async Task CircularDependenciesCheck(AssemblyModel assembly)
        {
            await busyService.RunActionAsync(async () =>
            {
                var assemblies = assembly.ReferenceProvider.Select(x => x.Value.LoadedAssembly)
                                                           .Distinct()
                                                           .Select(x => x.ToCheckModel())
                                                           .ToDictionary(x => x.Name);

                if (!assemblies.ContainsKey(assembly.Name))
                    assemblies.Add(assembly.Name, assembly.ToCheckModel());


                var service = new CircularReferenceCheck();

                var results = await service.AnalyseAsync(assembly.Name, assemblies).ConfigureAwait(true);

                var view = new CheckResultsView
                {
                    DataContext = new CheckResultsViewModel<CircularReferenceError>("Circular Dependencies results", results)
                };

                await DialogHost.Show(view).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task EntryPointNotFoundCheck(AssemblyModel assembly)
        {
            await busyService.RunActionAsync(async () =>
            {
                var assemblies = assembly.IsolatedShadowClone().ReferenceProvider
                                                               .Select(x => x.Value.LoadedAssembly)
                                                               .Distinct()
                                                               .Select(x => x.ToCheckModel())
                                                               .ToDictionary(x => x.Name);

                if (!assemblies.ContainsKey(assembly.Name))
                    assemblies.Add(assembly.Name, assembly.ToCheckModel());

                var service = new MissingEntryPointCheck();

                var results = await service.AnalyseAsync(assemblies).ConfigureAwait(true);

                var view = new CheckResultsView
                {
                    DataContext = new CheckResultsViewModel<MissingEntryPointError>("Missing entry point results", results)
                };

                await DialogHost.Show(view).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
