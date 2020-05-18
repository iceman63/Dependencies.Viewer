﻿using System.Linq;
using System.Threading.Tasks;
using Dependencies.Viewer.Wpf.Controls.Extensions;
using Dependencies.Viewer.Wpf.Controls.Models;
using MaterialDesignThemes.Wpf;

namespace Dependencies.Viewer.Wpf.Controls.ViewModels.Errors
{
    public abstract class ErrorListViewModel : ResultListViewModel<ReferenceModel>
    {
        protected override async Task OnOpenResultAsync(ReferenceModel item)
        {
            var paths = item.GetAssemblyParentPath(Assembly).ToList();
            var vm = new AssemblyParentsViewModel { BaseAssembly = item.AssemblyFullName, Paths = paths };

            _ = await DialogHost.Show(vm).ConfigureAwait(false);
        }
    }
}