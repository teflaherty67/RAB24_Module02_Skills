using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace RAB24_Module02_Skills
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document curDoc = uiapp.ActiveUIDocument.Document;

            // pick elements and filter them inot a list
            UIDocument uidoc = uiapp.ActiveUIDocument;
            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select elements");

            TaskDialog.Show("Test", "I selected " + pickList.Count + " elements");

            // filter selected elements for curves
            List<CurveElement> allCurves = new List<CurveElement>();
            foreach (Element curElem in pickList)
            {
                if (curElem is CurveElement)
                {
                    allCurves.Add(curElem as CurveElement);
                }
            }

            // filter selected elements for model curves
            List<CurveElement> modelCurves = new List<CurveElement>();
            foreach (Element curElem in pickList)
            {
                if (curElem is CurveElement)
                {
                    CurveElement curveElem = curElem as CurveElement;
                    // CurveElement curveElem = (CurveElement)curElem;

                    if(curveElem.CurveElementType == CurveElementType.ModelCurve)
                    {
                        modelCurves.Add(curveElem);
                    }
                }
            }

            // get curve data
            foreach (CurveElement curCurve in modelCurves)
            {
                Curve curve = curCurve.GeometryCurve;
                XYZ startPoint = curve.GetEndPoint(0);
                XYZ endPoint = curve.GetEndPoint(1);

                GraphicsStyle curStyle = curCurve.LineStyle as GraphicsStyle;

                Debug.Print(curStyle.Name);
            }

            // create transaction with using statement
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Create Revit elements");

                // create wall
                Level newLevel = Level.Create(curDoc, 20);
                Curve curCurve01 = modelCurves[0].GeometryCurve;

                Wall.Create(curDoc, curCurve01, newLevel.Id, false);

                FilteredElementCollector colWallTypes = new FilteredElementCollector(curDoc)
                    .OfClass(typeof(WallType));

                Curve curCurve02 = modelCurves[1].GeometryCurve;
                Wall.Create(curDoc, curCurve02, colWallTypes.FirstElementId(), newLevel.Id, 20, 0, false, false);

                // get system types
                FilteredElementCollector colSysTypes = new FilteredElementCollector(curDoc)
                    .OfClass(typeof(MEPSystemType));

                // get duct system type
                MEPSystemType ductSysType = null;
                foreach (MEPSystemType curType in colSysTypes)
                {
                    if (curType.Name == "Supply Air")
                    {
                        ductSysType = curType;
                        break;
                    }
                }

                // get duct type
                FilteredElementCollector colDuctTypes = new FilteredElementCollector(curDoc)
                    .OfClass(typeof(DuctType));

                // create duct
                Curve curCurve03 = modelCurves[2].GeometryCurve;

                Duct newDuct = Duct.Create(curDoc, ductSysType.Id, colDuctTypes.FirstElementId(),
                    newLevel.Id, curCurve03.GetEndPoint(0), curCurve03.GetEndPoint(1));

                // get pipe system type
                MEPSystemType pipeSysType = null;
                foreach (MEPSystemType curType in colSysTypes)
                {
                    if (curType.Name == "Domsetic Hot Water")
                    {
                        pipeSysType = curType;
                        break;
                    }
                }

                // get pipe type
                FilteredElementCollector colPipeTypes = new FilteredElementCollector(curDoc)
                    .OfClass(typeof(PipeType));

                // create duct
                Curve curCurve04 = modelCurves[3].GeometryCurve;

                Pipe newPipe = Pipe.Create(curDoc, pipeSysType.Id, colPipeTypes.FirstElementId(),
                    newLevel.Id, curCurve04.GetEndPoint(0), curCurve04.GetEndPoint(1));



                t.Commit();

            }


            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";
            string? methodBase = MethodBase.GetCurrentMethod().DeclaringType?.FullName;

            if (methodBase == null)
            {
                throw new InvalidOperationException("MethodBase.GetCurrentMethod().DeclaringType?.FullName is null");
            }
            else
            {
                Common.ButtonDataClass myButtonData1 = new Common.ButtonDataClass(
                    buttonInternalName,
                    buttonTitle,
                    methodBase,
                    Properties.Resources.Blue_32,
                    Properties.Resources.Blue_16,
                    "This is a tooltip for Button 1");

                return myButtonData1.Data;
            }
        }
    }

}
