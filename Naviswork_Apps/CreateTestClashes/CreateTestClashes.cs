using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Application = Autodesk.Navisworks.Api.Application;

namespace Naviswork_Apps
{
    public class CreateTestClashes: AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            Document doc = Application.ActiveDocument;

            if (doc == null || doc.IsClear)
            {
                MessageBox.Show("Abre un archivo de Navisworks antes de ejecutar la herramienta.", "Aviso");
                return 0;
            }

            // Obtener el módulo de Clash Detective
            DocumentClash clashDoc = doc.GetClash();
            DocumentClashTests clashTests = clashDoc.TestsData;

            // Extraer solo los Selection Sets de primer nivel
            List<SelectionSet> selectionSets = new List<SelectionSet>();
            foreach (SavedItem item in doc.SelectionSets.Value)
            {
                if (item is SelectionSet selSet)
                {
                    selectionSets.Add(selSet);
                }
            }

            if (selectionSets.Count < 2)
            {
                MessageBox.Show("Se necesitan al menos 2 Conjuntos de Selección (Selection Sets) creados.", "Atención");
                return 0;
            }

            int testCounter = 1;

            // Bucle N*(N-1)/2 para matriz de combinaciones únicas (evita duplicar A/B y B/A)
            for (int i = 0; i < selectionSets.Count; i++)
            {
                for (int j = i + 1; j < selectionSets.Count; j++)
                {
                    SelectionSet setA = selectionSets[i];
                    SelectionSet setB = selectionSets[j];

                    // Formato: 01. IIMM-HVAC/EST-COLUMNAS
                    string testName = $"{testCounter:D2}. {setA.DisplayName}/{setB.DisplayName}";

                    // Crear el nuevo Clash Test
                    ClashTest newTest = new ClashTest
                    {
                        DisplayName = testName,
                        TestType = ClashTestType.Hard,
                        Tolerance = 0.001 // Tolerancia en metros (1 mm)
                    };

                    // Asignar el Set A a la Selección A
                    newTest.SelectionA.Selection.SelectionSources.Add(
                        doc.SelectionSets.CreateSelectionSource(setA)
                    );

                    // Asignar el Set B a la Selección B
                    newTest.SelectionB.Selection.SelectionSources.Add(
                        doc.SelectionSets.CreateSelectionSource(setB)
                    );

                    // Agregar el test a la lista del Clash Detective
                    clashTests.TestsAddCopy(newTest);

                    testCounter++;
                }
            }

            MessageBox.Show($"¡Proceso finalizado! Se crearon {testCounter - 1} pruebas de clashes únicas.", "Éxito");
            return 0;
        }
    }
}
