using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forecast2Uwp.Thunderstore
{
    public class ThunderstorePageViewModel : AbstractViewModel
    {
        private const string AllPackagesCategoryKey = "All Categories";

        public Data.Package[] Packages { get; set; }
        public string[] AllCategories { get; set; }
        public ObservableCollection<Thunderstore.Data.Package> ShownPackages { get; set; }

        public ObservableCollection<Data.Package> UserSelectedPackages { get; set; }
        public ObservableCollection<Data.Package> DependentPackages { get; set; }
        public ObservableCollection<Data.Package> AllSelectedPackages => new ObservableCollection<Data.Package>(UserSelectedPackages.Union(DependentPackages));

        public ThunderstorePageViewModel()
        {
            Packages = (App.Current as App).ThunderstorePackages;
            ShownPackages = new ObservableCollection<Data.Package>(Packages);
            UserSelectedPackages = new ObservableCollection<Data.Package>();
            DependentPackages = new ObservableCollection<Data.Package>();
            AllCategories = Packages.SelectMany(x => x.Categories).Distinct().Append(AllPackagesCategoryKey).OrderBy(x => x).ToArray();
        }

        public void FilterPackages(string query, string category = AllPackagesCategoryKey)
        {
            var newFilteredItems = Packages.Where((pkg) =>
            {
                // Return packages with owner or fullName matching query string then further filter to those that match the selected category
                return (pkg.FullName.Contains(query) || pkg.Owner.Contains(query)) 
                && (pkg.Categories.Contains(category) || category == AllPackagesCategoryKey)
                && !AllSelectedPackages.Any(x => x.FullName == pkg.FullName);
            }).ToList();

            for (int i = ShownPackages.Count - 1; i >= 0; i--)
            {
                var item = ShownPackages[i];
                if (!newFilteredItems.Contains(item))
                {
                    ShownPackages.Remove(item);
                }
            }

            foreach (var item in newFilteredItems)
            {
                if (!ShownPackages.Contains(item))
                {
                    ShownPackages.Add(item);
                }
            }

            this.NotifyPropertyChanged(this, "ShownPackages");
        }

        private Data.Package GetPackageByFullVersionName(string name, bool singleVersion = true)
        {
            var pkg = Packages.FirstOrDefault(x => x.Versions.Any(y => y.FullName == name));
            if (pkg == null)
                return null;
            if (singleVersion)
                pkg.Versions = new Data.Version[] { pkg.Versions.First(y => y.FullName == name) };
            return pkg;
        }

        private List<Data.Package> GetDependenciesForPackage(Data.Package package)
        {
            return (from vs in package.Versions[0].Dependencies select GetPackageByFullVersionName(vs)).ToList();
        }

        public void DeselectPackage(Data.Package package)
        {
            UserSelectedPackages.Remove(package);
            RefreshDependencies();
        }

        public void SelectPackage(Data.Package package, string version = null)
        {
            Analytics.Log($"User selected Package {{ FullName = {package.FullName}, Version = {version} }}");
            if (version is null)
            {
                version = package.Versions.Select(v => new Version(v.VersionNumber)).OrderByDescending(x => x).First().ToString();
                Analytics.Log($"Auto selected latest Package {{ FullName = {package.FullName} }}.Version = {version}");
            }
                
            UserSelectedPackages.Add(package);
            RefreshDependencies();
        }

        private void RefreshDependencies()
        {
            var newDependencies = new List<Data.Package>();
            foreach (var userSelected in UserSelectedPackages)
            {
                newDependencies.AddRange(GetDependenciesForPackage(userSelected).Where(x => x != null));
            }

            UserSelectedPackages = new ObservableCollection<Data.Package>(UserSelectedPackages.Where(x => !newDependencies.Contains(x) && x != null));
            DependentPackages = new ObservableCollection<Data.Package>(newDependencies);

            Analytics.Log($"Refreshed Dependencies list");

            NotifyPropertyChanged(this, "ShownPackages", "UserSelectedPackages", "DependentPackages", "AllSelectedPackages");
        }
    }
}
