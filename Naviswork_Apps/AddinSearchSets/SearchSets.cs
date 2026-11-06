using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using Application = Autodesk.Navisworks.Api.Application;

namespace Naviswork_Apps.AddinSearchSets
{
    /// <summary>
    /// Crea una busqueda (Search Set) por cada valor distinto de la propiedad
    /// PR-Elemento encontrado en los modelos elegidos por el usuario.
    ///
    /// El nombre de cada busqueda es: {sexta parte del nombre del modelo}-{valor PR-Elemento}
    /// Ej.: modelo "2657134-COL-003-XXX-BIM-EST-0003"  ->  prefijo "EST"
    ///      valores "Columna", "Viga"                  ->  "EST-Columna", "EST-Viga"
    /// Todas las busquedas de un modelo se guardan dentro de una carpeta con el prefijo ("EST").
    ///
    /// Si el modelo ya tiene su carpeta, solo se crean las busquedas que faltan
    /// (valores de PR-Elemento que aparecieron despues, p.ej. al agregar elementos).
    /// </summary>
    public class SearchSets : AddInPlugin
    {
        public const string TargetPropertyName = "PR-Elemento";

        public override int Execute(params string[] parameters)
        {
            Document doc = Application.ActiveDocument;

            if (doc == null || doc.IsClear)
            {
                MessageBox.Show("Abre un archivo de Navisworks antes de ejecutar la herramienta.", "Aviso");
                return 0;
            }

            if (doc.Models.Count == 0)
            {
                MessageBox.Show("El documento no contiene modelos.", "Aviso");
                return 0;
            }

            ViewSearchSets view = new ViewSearchSets();
            IntPtr handle = Application.Gui.MainWindow.Handle;
            new WindowInteropHelper(view).Owner = handle;
            view.Show();

            return 0;
        }

        /// <summary>
        /// Calcula, sin modificar nada, que busquedas faltan y cuales ya existen
        /// para cada modelo indicado.
        /// </summary>
        public static SearchSetsResult BuildPlan(Document doc, IEnumerable<Model> models)
        {
            var result = new SearchSetsResult();

            foreach (Model model in models)
            {
                ModelPlan plan;
                string categoryName, propertyName;
                HashSet<string> values;

                if (!TryDiscover(model, out plan, out categoryName, out propertyName, out values))
                {
                    result.Plans.Add(plan);
                    continue;
                }

                FolderItem folder = FindFolder(doc, plan.Prefix);
                foreach (string value in OrderValues(values))
                {
                    string setName = plan.Prefix + "-" + value;
                    if (folder != null && FolderContains(folder, setName))
                        plan.ExistingSets.Add(setName);
                    else
                        plan.MissingValues.Add(value);
                }

                result.Plans.Add(plan);
            }

            return result;
        }

        /// <summary>
        /// Crea unicamente las busquedas que faltan. Reutiliza la carpeta del prefijo
        /// si ya existe y no toca el resto del arbol de conjuntos de seleccion.
        /// </summary>
        public static SearchSetsResult CreateForModels(Document doc, IEnumerable<Model> models)
        {
            var result = new SearchSetsResult();

            foreach (Model model in models)
            {
                ModelPlan plan;
                string categoryName, propertyName;
                HashSet<string> values;

                if (!TryDiscover(model, out plan, out categoryName, out propertyName, out values))
                {
                    result.Plans.Add(plan);
                    continue;
                }

                FolderItem folder = FindFolder(doc, plan.Prefix);
                if (folder == null)
                {
                    doc.SelectionSets.AddCopy(new FolderItem { DisplayName = plan.Prefix });
                    result.CreatedFolders++;
                    folder = FindFolder(doc, plan.Prefix);
                }

                if (folder == null)
                {
                    plan.SkipReason = "no se pudo crear la carpeta '" + plan.Prefix + "'";
                    result.Plans.Add(plan);
                    continue;
                }

                foreach (string value in OrderValues(values))
                {
                    string setName = plan.Prefix + "-" + value;
                    if (FolderContains(folder, setName))
                    {
                        plan.ExistingSets.Add(setName);
                        continue;
                    }

                    Search search = BuildSearch(model, categoryName, propertyName, value);
                    SelectionSet set = new SelectionSet(search) { DisplayName = setName };

                    doc.SelectionSets.InsertCopy(folder, folder.Children.Count, set);
                    result.CreatedSets++;
                    plan.CreatedValues.Add(value);
                }

                result.Plans.Add(plan);
            }

            return result;
        }

        /// <summary>
        /// Descubre prefijo, categoria/propiedad internas y valores distintos de
        /// PR-Elemento del modelo. Devuelve false y llena <paramref name="plan"/>.SkipReason
        /// cuando el modelo no se puede procesar.
        /// </summary>
        private static bool TryDiscover(Model model, out ModelPlan plan,
                                        out string categoryName, out string propertyName,
                                        out HashSet<string> values)
        {
            string modelName = GetModelName(model);
            string prefix = GetSixthPart(modelName);

            plan = new ModelPlan { ModelName = modelName, Prefix = prefix };
            categoryName = null;
            propertyName = null;
            values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(prefix))
            {
                plan.SkipReason = "el nombre no tiene 6 partes separadas por '-'";
                return false;
            }

            foreach (ModelItem item in model.RootItem.DescendantsAndSelf)
            {
                if (categoryName != null)
                {
                    // Ruta rapida: ya conocemos la categoria/propiedad.
                    AddValue(values, item.PropertyCategories.FindPropertyByName(categoryName, propertyName));
                    continue;
                }

                foreach (PropertyCategory category in item.PropertyCategories)
                {
                    DataProperty match = category.Properties.FirstOrDefault(p => IsTargetProperty(category, p));
                    if (match == null) continue;

                    categoryName = category.CombinedName.Name;
                    propertyName = match.CombinedName.Name;
                    AddValue(values, match);
                    break;
                }
            }

            if (values.Count == 0)
            {
                plan.SkipReason = "sin propiedad '" + TargetPropertyName + "'";
                return false;
            }

            return true;
        }

        private static IEnumerable<string> OrderValues(HashSet<string> values)
        {
            return values.OrderBy(v => v, StringComparer.CurrentCultureIgnoreCase);
        }

        private static void AddValue(HashSet<string> values, DataProperty prop)
        {
            if (prop == null) return;
            string value = prop.Value != null ? prop.Value.ToDisplayString() : null;
            if (!string.IsNullOrWhiteSpace(value)) values.Add(value.Trim());
        }

        /// <summary>
        /// Coincide cuando el nombre visible de la propiedad es "PR-Elemento",
        /// o cuando la categoria es "PR" y la propiedad "Elemento".
        /// </summary>
        private static bool IsTargetProperty(PropertyCategory category, DataProperty prop)
        {
            if (prop == null) return false;

            if (Eq(prop.DisplayName, TargetPropertyName)) return true;
            if (prop.CombinedName != null && Eq(prop.CombinedName.DisplayName, TargetPropertyName)) return true;
            if (Eq(category.DisplayName, "PR") && Eq(prop.DisplayName, "Elemento")) return true;

            return false;
        }

        private static bool Eq(string a, string b)
        {
            return string.Equals(a == null ? null : a.Trim(), b, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetModelName(Model model)
        {
            string name = model.RootItem != null ? model.RootItem.DisplayName : null;
            if (!string.IsNullOrWhiteSpace(name)) return name.Trim();

            try { return Path.GetFileNameWithoutExtension(model.FileName); }
            catch { return model.FileName ?? "(modelo sin nombre)"; }
        }

        /// <summary>
        /// Sexta parte del nombre del modelo separado por '-'.
        /// "2657134-COL-003-XXX-BIM-EST-0003" -> "EST".
        /// </summary>
        public static string GetSixthPart(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName)) return null;

            modelName = Path.GetFileNameWithoutExtension(modelName);
            string[] parts = modelName.Split('-');
            if (parts.Length < 6) return null;

            return parts[5].Trim();
        }

        private static Search BuildSearch(Model model, string categoryName, string propertyName, string value)
        {
            Search search = new Search();

            // Limita la busqueda a este modelo.
            search.Selection.CopyFrom(new ModelItemCollection { model.RootItem });
            search.Locations = SearchLocations.DescendantsAndSelf;

            SearchCondition condition =
                SearchCondition.HasPropertyByName(categoryName, propertyName)
                               .EqualValue(VariantData.FromDisplayString(value));

            search.SearchConditions.Add(condition);
            return search;
        }

        private static FolderItem FindFolder(Document doc, string name)
        {
            foreach (SavedItem item in doc.SelectionSets.Value)
            {
                FolderItem folder = item as FolderItem;
                if (folder != null && Eq(folder.DisplayName, name))
                    return folder;
            }
            return null;
        }

        private static bool FolderContains(FolderItem folder, string setName)
        {
            foreach (SavedItem child in folder.Children)
            {
                if (Eq(child.DisplayName, setName)) return true;
            }
            return false;
        }
    }

    /// <summary>Plan / resultado por modelo.</summary>
    public class ModelPlan
    {
        public string ModelName;
        public string Prefix;
        public string SkipReason;

        /// <summary>Busquedas que ya estaban en la carpeta (no se tocan).</summary>
        public readonly List<string> ExistingSets = new List<string>();

        /// <summary>Valores sin busqueda (los llena BuildPlan).</summary>
        public readonly List<string> MissingValues = new List<string>();

        /// <summary>Valores para los que se creo una busqueda (los llena CreateForModels).</summary>
        public readonly List<string> CreatedValues = new List<string>();
    }

    /// <summary>Resumen de <see cref="SearchSets.BuildPlan"/> / <see cref="SearchSets.CreateForModels"/>.</summary>
    public class SearchSetsResult
    {
        public int CreatedFolders;
        public int CreatedSets;
        public readonly List<ModelPlan> Plans = new List<ModelPlan>();

        public int TotalMissing { get { return Plans.Sum(p => p.MissingValues.Count); } }
        public int TotalExisting { get { return Plans.Sum(p => p.ExistingSets.Count); } }
        public int TotalSkipped { get { return Plans.Count(p => p.SkipReason != null); } }

        public string ToLog(bool preview)
        {
            var sb = new StringBuilder();

            foreach (ModelPlan p in Plans)
            {
                string head = string.IsNullOrEmpty(p.Prefix) ? "?" : p.Prefix;
                sb.AppendLine("[" + head + "]  " + p.ModelName);

                if (p.SkipReason != null)
                {
                    sb.AppendLine("    omitido: " + p.SkipReason);
                    sb.AppendLine();
                    continue;
                }

                List<string> target = preview ? p.MissingValues : p.CreatedValues;
                string label = preview ? "faltan" : "creadas";

                if (target.Count > 0)
                    sb.AppendLine("    " + label + " (" + target.Count + "): " +
                                  string.Join(", ", target.Select(v => p.Prefix + "-" + v)));
                else
                    sb.AppendLine("    " + label + " (0)");

                if (p.ExistingSets.Count > 0)
                    sb.AppendLine("    ya existian (" + p.ExistingSets.Count + "): " +
                                  string.Join(", ", p.ExistingSets));

                sb.AppendLine();
            }

            if (Plans.Count == 0) sb.AppendLine("Nada que procesar.");
            return sb.ToString();
        }
    }
}
