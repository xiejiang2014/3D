//#define IRB6700

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using System.IO;


/**
 * Author: Gabriele Marini (Gabryxx7)
 * This class load the 3d models of all the parts of the robotic arms and add them to the viewport
 * It also defines the relations among the joints of the robotic arms in order to reflect the movement of the robot in the real world
 * **/
namespace RobotArmHelix
{
    class Joint
    {
        public readonly Model3D Model     = null;
        public          double  Angle     = 0;
        public          double  AngleMin  = -180;
        public          double  AngleMax  = 180;
        public          int     RotPointX = 0;
        public          int     RotPointY = 0;
        public          int     RotPointZ = 0;
        public          int     RotAxisX  = 0;
        public          int     RotAxisY  = 0;
        public          int     RotAxisZ  = 0;

        public Joint(Model3D pModel)
        {
            Model = pModel;
        }
    }

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //provides functionality to 3d models
        Model3DGroup _ra   = new Model3DGroup(); //RoboticArm 3d group
        Model3D      _geom = null;               //Debug sphere to check in which point the joint is rotatin

        List<Joint> _joints = null;

        bool _switchingJoint = false;
        bool _isAnimating    = false;

        Color           _oldColor         = Colors.White;
        GeometryModel3D _oldSelectedModel = null;
        string          _basePath         = "";
        ModelVisual3D   _visual;
        double          _learningRate     = 0.01;
        double          _samplingDistance = 0.15;

        double _distanceThreshold = 20;

        //provides render to model3d objects
        ModelVisual3D              _roboticArm = new ModelVisual3D();
        Transform3DGroup           _f1;
        Transform3DGroup           _f2;
        Transform3DGroup           _f3;
        Transform3DGroup           _f4;
        Transform3DGroup           _f5;
        Transform3DGroup           _f6;
        RotateTransform3D          _r;
        TranslateTransform3D       _;
        Vector3D                   _reachingPoint;
        int                        _movements = 10;
        System.Windows.Forms.Timer _timer1;

#if IRB6700
        //directroy of all stl files
        private const string MODEL_PATH1 = "IRB6700-MH3_245-300_IRC5_rev02_LINK01_CAD.stl";
        private const string MODEL_PATH2 = "IRB6700-MH3_245-300_IRC5_rev00_LINK02_CAD.stl";
        private const string MODEL_PATH3 = "IRB6700-MH3_245-300_IRC5_rev02_LINK03_CAD.stl";
        private const string MODEL_PATH4 = "IRB6700-MH3_245-300_IRC5_rev01_LINK04_CAD.stl";
        private const string MODEL_PATH5 = "IRB6700-MH3_245-300_IRC5_rev01_LINK05_CAD.stl";
        private const string MODEL_PATH6 = "IRB6700-MH3_245-300_IRC5_rev01_LINK06_CAD.stl";
        private const string MODEL_PATH7 = "IRB6700-MH3_245-300_IRC5_rev02_LINK01_CABLE.stl";
        private const string MODEL_PATH8 = "IRB6700-MH3_245-300_IRC5_rev02_LINK01m_CABLE.stl";
        private const string MODEL_PATH9 = "IRB6700-MH3_245-300_IRC5_rev00_LINK02_CABLE.stl";
        private const string MODEL_PATH10 = "IRB6700-MH3_245-300_IRC5_rev00_LINK02m_CABLE.stl";
        private const string MODEL_PATH11 = "IRB6700-MH3_245-300_IRC5_rev00_LINK03a_CABLE.stl";
        private const string MODEL_PATH12 = "IRB6700-MH3_245-300_IRC5_rev00_LINK03b_CABLE.stl";
        private const string MODEL_PATH13 = "IRB6700-MH3_245-300_IRC5_rev02_LINK03m_CABLE.stl";
        private const string MODEL_PATH14 = "IRB6700-MH3_245-300_IRC5_rev01_LINK04_CABLE.stl";
        private const string MODEL_PATH15 = "IRB6700-MH3_245-300_IRC5_rev00_ROD_CAD.stl";
        private const string MODEL_PATH16 = "IRB6700-MH3_245-300_IRC5_rev00_LOGO1_CAD.stl";
        private const string MODEL_PATH17 = "IRB6700-MH3_245-300_IRC5_rev00_LOGO2_CAD.stl";
        private const string MODEL_PATH18 = "IRB6700-MH3_245-300_IRC5_rev00_LOGO3_CAD.stl";
        private const string MODEL_PATH19 = "IRB6700-MH3_245-300_IRC5_rev01_BASE_CAD.stl";
        private const string MODEL_PATH20 = "IRB6700-MH3_245-300_IRC5_rev00_CYLINDER_CAD.stl";
#else

        private const string ModelPath1  = "IRB4600_20kg-250_LINK1_CAD_rev04.stl";
        private const string ModelPath2  = "IRB4600_20kg-250_LINK2_CAD_rev04.stl";
        private const string ModelPath3  = "IRB4600_20kg-250_LINK3_CAD_rev005.stl";
        private const string ModelPath4  = "IRB4600_20kg-250_LINK4_CAD_rev04.stl";
        private const string ModelPath5  = "IRB4600_20kg-250_LINK5_CAD_rev04.stl";
        private const string ModelPath6  = "IRB4600_20kg-250_LINK6_CAD_rev04.stl";
        private const string ModelPath7  = "IRB4600_20kg-250_LINK3_CAD_rev04.stl";
        private const string ModelPath8  = "IRB4600_20kg-250_CABLES_LINK1_rev03.stl";
        private const string ModelPath9  = "IRB4600_20kg-250_CABLES_LINK2_rev03.stl";
        private const string ModelPath10 = "IRB4600_20kg-250_CABLES_LINK3_rev03.stl";
        private const string ModelPath11 = "IRB4600_20kg-250_BASE_CAD_rev04.stl";
#endif


        public MainWindow()
        {
            InitializeComponent();
            _basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\3D_Models\\";
            List<string> modelsNames = new List<string>();
            modelsNames.Add(ModelPath1);
            modelsNames.Add(ModelPath2);
            modelsNames.Add(ModelPath3);
            modelsNames.Add(ModelPath4);
            modelsNames.Add(ModelPath5);
            modelsNames.Add(ModelPath6);
            modelsNames.Add(ModelPath7);
            modelsNames.Add(ModelPath8);
            modelsNames.Add(ModelPath9);
            modelsNames.Add(ModelPath10);
            modelsNames.Add(ModelPath11); //Until here for the 4600
#if IRB6700
            modelsNames.Add(MODEL_PATH12);
            modelsNames.Add(MODEL_PATH13);
            modelsNames.Add(MODEL_PATH14);
            modelsNames.Add(MODEL_PATH15);
            modelsNames.Add(MODEL_PATH16);
            modelsNames.Add(MODEL_PATH17);
            modelsNames.Add(MODEL_PATH18);
            modelsNames.Add(MODEL_PATH19);
            modelsNames.Add(MODEL_PATH20);
#endif

            //加载模型
            (_ra, _joints) = Initialize_Environment(modelsNames, _basePath,_oldColor);

            _roboticArm.Content = _ra;

            /** Debug sphere to check in which point the joint is rotating**/
            var builder  = new MeshBuilder(true, true);
            var position = new Point3D(0, 0, 0);
            builder.AddSphere(position, 50, 15, 15);
            _geom           = new GeometryModel3D(builder.ToMesh(), Materials.Brown);
            _visual         = new ModelVisual3D();
            _visual.Content = _geom;

            viewPort3d.RotateGesture = new MouseGesture(MouseAction.RightClick);
            viewPort3d.PanGesture    = new MouseGesture(MouseAction.LeftClick);
            viewPort3d.Children.Add(_visual);
            viewPort3d.Children.Add(_roboticArm);
            viewPort3d.Camera.LookDirection = new Vector3D(2038,   -5200, -2930);
            viewPort3d.Camera.UpDirection   = new Vector3D(-0.145, 0.372, 0.917);
            viewPort3d.Camera.Position      = new Point3D(-1571, 4801, 3774);

            double[] angles =
            {
                _joints[0].Angle, _joints[1].Angle, _joints[2].Angle, _joints[3].Angle, _joints[4].Angle,
                _joints[5].Angle
            };
            ForwardKinematics(angles);

            ChangeSelectedJoint();

            _timer1          =  new System.Windows.Forms.Timer();
            _timer1.Interval =  5;
            _timer1.Tick     += new System.EventHandler(timer1_Tick);
        }

        #region 加载模型

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelsNames">模型文件列表</param>
        /// <param name="basePath"></param>
        /// <param name="oldColor"></param>
        /// <returns></returns>
        private static (Model3DGroup model3DGroup, List<Joint> joints) Initialize_Environment(
            List<string> modelsNames, string basePath, Color oldColor)
        {
            List<Joint> joints = new List<Joint>();
            var         ra     = new Model3DGroup();

            try
            {
                ModelImporter import = new ModelImporter();

                foreach (string modelName in modelsNames)
                {
                    //创建材质
                    var   materialGroup = new MaterialGroup();
                    Color mainColor     = Colors.White;

                    //放射性材料
                    EmissiveMaterial emissMat = new EmissiveMaterial(new SolidColorBrush(mainColor));

                    //漫反射材质
                    DiffuseMaterial diffMat = new DiffuseMaterial(new SolidColorBrush(mainColor));

                    //高光材质
                    SpecularMaterial specMat = new SpecularMaterial(new SolidColorBrush(mainColor), 200);
                    materialGroup.Children.Add(emissMat);
                    materialGroup.Children.Add(diffMat);
                    materialGroup.Children.Add(specMat);


                    var model3DGroup = import.Load(basePath + modelName); //从模型文件加载模型组

                    //为模型组中的首个模型设置材质
                    GeometryModel3D model = model3DGroup.Children[0] as GeometryModel3D;
                    model.Material     = materialGroup; //正面材质
                    model.BackMaterial = materialGroup; //背面材质

                    //所有的模型组至中都加入到了 joints 中( 注意 Model3DGroup : Model3D)
                    joints.Add(new Joint(model3DGroup)); //
                }

                //又将所有的模型加入到了 RA 中
                ra.Children.Add(joints[0].Model);
                ra.Children.Add(joints[1].Model);
                ra.Children.Add(joints[2].Model);
                ra.Children.Add(joints[3].Model);
                ra.Children.Add(joints[4].Model);
                ra.Children.Add(joints[5].Model);
                ra.Children.Add(joints[6].Model);
                ra.Children.Add(joints[7].Model);
                ra.Children.Add(joints[8].Model);
                ra.Children.Add(joints[9].Model);
                ra.Children.Add(joints[10].Model);
#if IRB6700
                RA.Children.Add(joints[11].model);
                RA.Children.Add(joints[12].model);
                RA.Children.Add(joints[13].model);
                RA.Children.Add(joints[14].model);
                RA.Children.Add(joints[15].model);
                RA.Children.Add(joints[16].model);
                RA.Children.Add(joints[17].model);
                RA.Children.Add(joints[18].model);
                RA.Children.Add(joints[19].model);
#endif

#if IRB6700
                Color cableColor = Colors.DarkSlateGray;
                changeModelColor(joints[6], cableColor);
                changeModelColor(joints[7], cableColor);
                changeModelColor(joints[8], cableColor);
                changeModelColor(joints[9], cableColor);
                changeModelColor(joints[10], cableColor);
                changeModelColor(joints[11], cableColor);
                changeModelColor(joints[12], cableColor);
                changeModelColor(joints[13], cableColor);

                changeModelColor(joints[14], Colors.Gray);

                changeModelColor(joints[15], Colors.Red);
                changeModelColor(joints[16], Colors.Red);
                changeModelColor(joints[17], Colors.Red);

                changeModelColor(joints[18], Colors.Gray);
                changeModelColor(joints[19], Colors.Gray);

                joints[0].angleMin = -180;
                joints[0].angleMax = 180;
                joints[0].rotAxisX = 0;
                joints[0].rotAxisY = 0;
                joints[0].rotAxisZ = 1;
                joints[0].rotPointX = 0;
                joints[0].rotPointY = 0;
                joints[0].rotPointZ = 0;

                joints[1].angleMin = -100;
                joints[1].angleMax = 60;
                joints[1].rotAxisX = 0;
                joints[1].rotAxisY = 1;
                joints[1].rotAxisZ = 0;
                joints[1].rotPointX = 348;
                joints[1].rotPointY = -243;
                joints[1].rotPointZ = 775;

                joints[2].angleMin = -90;
                joints[2].angleMax = 90;
                joints[2].rotAxisX = 0;
                joints[2].rotAxisY = 1;
                joints[2].rotAxisZ = 0;
                joints[2].rotPointX = 347;
                joints[2].rotPointY = -376;
                joints[2].rotPointZ = 1923;

                joints[3].angleMin = -180;
                joints[3].angleMax = 180;
                joints[3].rotAxisX = 1;
                joints[3].rotAxisY = 0;
                joints[3].rotAxisZ = 0;
                joints[3].rotPointX = 60;
                joints[3].rotPointY = 0;
                joints[3].rotPointZ = 2125;

                joints[4].angleMin = -115;
                joints[4].angleMax = 115;
                joints[4].rotAxisX = 0;
                joints[4].rotAxisY = 1;
                joints[4].rotAxisZ = 0;
                joints[4].rotPointX = 1815;
                joints[4].rotPointY = 0;
                joints[4].rotPointZ = 2125;

                joints[5].angleMin = -180;
                joints[5].angleMax = 180;
                joints[5].rotAxisX = 1;
                joints[5].rotAxisY = 0;
                joints[5].rotAxisZ = 0;
                joints[5].rotPointX = 2008;
                joints[5].rotPointY = 0;
                joints[5].rotPointZ = 2125;

#else
                //针对性的设置颜色
                ChangeModelColor(joints[6],  Colors.Red,   oldColor);
                ChangeModelColor(joints[7],  Colors.Black, oldColor);
                ChangeModelColor(joints[8],  Colors.Black, oldColor);
                ChangeModelColor(joints[9],  Colors.Black, oldColor);
                ChangeModelColor(joints[10], Colors.Gray,  oldColor);

                // ?? 为什么又加了一遍
                ra.Children.Add(joints[0].Model);
                ra.Children.Add(joints[1].Model);
                ra.Children.Add(joints[2].Model);
                ra.Children.Add(joints[3].Model);
                ra.Children.Add(joints[4].Model);
                ra.Children.Add(joints[5].Model);
                ra.Children.Add(joints[6].Model);
                ra.Children.Add(joints[7].Model);
                ra.Children.Add(joints[8].Model);
                ra.Children.Add(joints[9].Model);
                ra.Children.Add(joints[10].Model);

                //这看起来是每个可活动关节的限位
                joints[0].AngleMin  = -180;
                joints[0].AngleMax  = 180;
                joints[0].RotAxisX  = 0;
                joints[0].RotAxisY  = 0;
                joints[0].RotAxisZ  = 1;
                joints[0].RotPointX = 0;
                joints[0].RotPointY = 0;
                joints[0].RotPointZ = 0;

                joints[1].AngleMin  = -100;
                joints[1].AngleMax  = 60;
                joints[1].RotAxisX  = 0;
                joints[1].RotAxisY  = 1;
                joints[1].RotAxisZ  = 0;
                joints[1].RotPointX = 175;
                joints[1].RotPointY = -200;
                joints[1].RotPointZ = 500;

                joints[2].AngleMin  = -90;
                joints[2].AngleMax  = 90;
                joints[2].RotAxisX  = 0;
                joints[2].RotAxisY  = 1;
                joints[2].RotAxisZ  = 0;
                joints[2].RotPointX = 190;
                joints[2].RotPointY = -700;
                joints[2].RotPointZ = 1595;

                joints[3].AngleMin  = -180;
                joints[3].AngleMax  = 180;
                joints[3].RotAxisX  = 1;
                joints[3].RotAxisY  = 0;
                joints[3].RotAxisZ  = 0;
                joints[3].RotPointX = 400;
                joints[3].RotPointY = 0;
                joints[3].RotPointZ = 1765;

                joints[4].AngleMin  = -115;
                joints[4].AngleMax  = 115;
                joints[4].RotAxisX  = 0;
                joints[4].RotAxisY  = 1;
                joints[4].RotAxisZ  = 0;
                joints[4].RotPointX = 1405;
                joints[4].RotPointY = 50;
                joints[4].RotPointZ = 1765;

                joints[5].AngleMin  = -180;
                joints[5].AngleMax  = 180;
                joints[5].RotAxisX  = 1;
                joints[5].RotAxisY  = 0;
                joints[5].RotAxisZ  = 0;
                joints[5].RotPointX = 1405;
                joints[5].RotPointY = 0;
                joints[5].RotPointZ = 1765;
#endif
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception Error:" + e.StackTrace);
            }

            return (ra, joints);
        }
        

        #endregion
        public static T Clamp<T>(T value, T min, T max)
            where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }

        private void ReachingPoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _reachingPoint  = new Vector3D(Double.Parse(TbX.Text), Double.Parse(TbY.Text), Double.Parse(TbZ.Text));
                _geom.Transform = new TranslateTransform3D(_reachingPoint);
            }
            catch (Exception exc)
            {
            }
        }

        private void jointSelector_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeSelectedJoint();
        }

        private void ChangeSelectedJoint()
        {
            if (_joints == null)
                return;

            int sel = ((int)jointSelector.Value) - 1;
            _switchingJoint = true;
            UnselectModel();
            if (sel < 0)
            {
                jointX.IsEnabled     = false;
                jointY.IsEnabled     = false;
                jointZ.IsEnabled     = false;
                jointXAxis.IsEnabled = false;
                jointYAxis.IsEnabled = false;
                jointZAxis.IsEnabled = false;
            }
            else
            {
                if (!jointX.IsEnabled)
                {
                    jointX.IsEnabled     = true;
                    jointY.IsEnabled     = true;
                    jointZ.IsEnabled     = true;
                    jointXAxis.IsEnabled = true;
                    jointYAxis.IsEnabled = true;
                    jointZAxis.IsEnabled = true;
                }

                jointX.Value         = _joints[sel].RotPointX;
                jointY.Value         = _joints[sel].RotPointY;
                jointZ.Value         = _joints[sel].RotPointZ;
                jointXAxis.IsChecked = _joints[sel].RotAxisX == 1 ? true : false;
                jointYAxis.IsChecked = _joints[sel].RotAxisY == 1 ? true : false;
                jointZAxis.IsChecked = _joints[sel].RotAxisZ == 1 ? true : false;
                SelectModel(_joints[sel].Model);
                UpdateSpherePosition();
            }

            _switchingJoint = false;
        }

        private void RotationPointChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_switchingJoint)
                return;

            int sel = ((int)jointSelector.Value) - 1;
            _joints[sel].RotPointX = (int)jointX.Value;
            _joints[sel].RotPointY = (int)jointY.Value;
            _joints[sel].RotPointZ = (int)jointZ.Value;
            UpdateSpherePosition();
        }

        private void UpdateSpherePosition()
        {
            int sel = ((int)jointSelector.Value) - 1;
            if (sel < 0)
                return;

            Transform3DGroup f = new Transform3DGroup();
            f.Children.Add(new TranslateTransform3D(_joints[sel].RotPointX, _joints[sel].RotPointY,
                                                    _joints[sel].RotPointZ));
            f.Children.Add(_joints[sel].Model.Transform);
            _geom.Transform = f;
        }

        private void joint_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isAnimating)
                return;

            _joints[0].Angle = joint1.Value;
            _joints[1].Angle = joint2.Value;
            _joints[2].Angle = joint3.Value;
            _joints[3].Angle = joint4.Value;
            _joints[4].Angle = joint5.Value;
            _joints[5].Angle = joint6.Value;
            execute_fk();
        }


        private void CheckBox_StateChanged(object sender, RoutedEventArgs e)
        {
            if (_switchingJoint)
                return;

            int sel = ((int)jointSelector.Value) - 1;
            _joints[sel].RotAxisX = jointXAxis.IsChecked.Value ? 1 : 0;
            _joints[sel].RotAxisY = jointYAxis.IsChecked.Value ? 1 : 0;
            _joints[sel].RotAxisZ = jointZAxis.IsChecked.Value ? 1 : 0;
        }


        /**
         * This methodes execute the FK (Forward Kinematics). It starts from the first joint, the base.
         * */
        private void execute_fk()
        {
            /** Debug sphere, it takes the x,y,z of the textBoxes and update its position
             * This is useful when using x,y,z in the "new Point3D(x,y,z)* when defining a new RotateTransform3D() to check where the joints is actually  rotating */
            double[] angles =
            {
                _joints[0].Angle, _joints[1].Angle, _joints[2].Angle, _joints[3].Angle, _joints[4].Angle,
                _joints[5].Angle
            };
            ForwardKinematics(angles);
            UpdateSpherePosition();
        }

        private static Color ChangeModelColor(Joint pJoint, Color newColor, Color oldColor)
        {
            Model3DGroup models = ((Model3DGroup)pJoint.Model);
            return ChangeModelColor(models.Children[0] as GeometryModel3D, newColor, oldColor);
        }

        private static Color ChangeModelColor(GeometryModel3D pModel, Color newColor,Color           oldColor)
        {
            if (pModel == null)
                return oldColor;

            Color previousColor = Colors.Black;

            MaterialGroup mg = (MaterialGroup)pModel.Material;
            if (mg.Children.Count > 0)
            {
                try
                {
                    previousColor                            = ((EmissiveMaterial)mg.Children[0]).Color;
                    ((EmissiveMaterial)mg.Children[0]).Color = newColor;
                    ((DiffuseMaterial)mg.Children[1]).Color  = newColor;
                }
                catch (Exception exc)
                {
                    previousColor = oldColor;
                }
            }

            return previousColor;
        }


        private void SelectModel(Model3D pModel)
        {
            try
            {
                Model3DGroup models = ((Model3DGroup)pModel);
                _oldSelectedModel = models.Children[0] as GeometryModel3D;
            }
            catch (Exception exc)
            {
                _oldSelectedModel = (GeometryModel3D)pModel;
            }

            _oldColor = ChangeModelColor(_oldSelectedModel, ColorHelper.HexToColor("#ff3333"),_oldColor);
        }

        private void UnselectModel()
        {
            ChangeModelColor(_oldSelectedModel, _oldColor, _oldColor);
        }

        private void ViewPort3D_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point                  mousePos  = e.GetPosition(viewPort3d);
            PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            VisualTreeHelper.HitTest(viewPort3d, null, ResultCallback, hitParams);
        }

        private void ViewPort3D_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Perform the hit test on the mouse's position relative to the viewport.
            HitTestResult                  result = VisualTreeHelper.HitTest(viewPort3d, e.GetPosition(viewPort3d));
            RayMeshGeometry3DHitTestResult meshResult = result as RayMeshGeometry3DHitTestResult;

            if (_oldSelectedModel != null)
                UnselectModel();

            if (meshResult != null)
            {
                SelectModel(meshResult.ModelHit);
            }
        }

        public HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            // Did we hit 3D?
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                RayMeshGeometry3DHitTestResult rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;
                _geom.Transform =
                    new TranslateTransform3D(new Vector3D(rayResult.PointHit.X, rayResult.PointHit.Y,
                                                          rayResult.PointHit.Z));

                if (rayMeshResult != null)
                {
                    // Yes we did!
                }
            }

            return HitTestResultBehavior.Continue;
        }

        public void StartInverseKinematics(object sender, RoutedEventArgs e)
        {
            if (_timer1.Enabled)
            {
                button.Content = "Go to position";
                _isAnimating    = false;
                _timer1.Stop();
                _movements = 0;
            }
            else
            {
                _geom.Transform = new TranslateTransform3D(_reachingPoint);
                _movements      = 5000;
                button.Content = "STOP";
                _isAnimating    = true;
                _timer1.Start();
            }
        }

        public void timer1_Tick(object sender, EventArgs e)
        {
            double[] angles =
            {
                _joints[0].Angle, _joints[1].Angle, _joints[2].Angle, _joints[3].Angle, _joints[4].Angle,
                _joints[5].Angle
            };
            angles       = InverseKinematics(_reachingPoint, angles);
            joint1.Value = _joints[0].Angle = angles[0];
            joint2.Value = _joints[1].Angle = angles[1];
            joint3.Value = _joints[2].Angle = angles[2];
            joint4.Value = _joints[3].Angle = angles[3];
            joint5.Value = _joints[4].Angle = angles[4];
            joint6.Value = _joints[5].Angle = angles[5];

            if ((--_movements) <= 0)
            {
                button.Content = "Go to position";
                _isAnimating    = false;
                _timer1.Stop();
            }
        }

        public double[] InverseKinematics(Vector3D target, double[] angles)
        {
            if (DistanceFromTarget(target, angles) < _distanceThreshold)
            {
                _movements = 0;
                return angles;
            }

            double[] oldAngles = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            angles.CopyTo(oldAngles, 0);
            for (int i = 0; i <= 5; i++)
            {
                // Gradient descent
                // Update : Solution -= LearningRate * Gradient
                double gradient = PartialGradient(target, angles, i);
                angles[i] -= _learningRate * gradient;

                // Clamp
                angles[i] = Clamp(angles[i], _joints[i].AngleMin, _joints[i].AngleMax);

                // Early termination
                if (DistanceFromTarget(target, angles) < _distanceThreshold || CheckAngles(oldAngles, angles))
                {
                    _movements = 0;
                    return angles;
                }
            }

            return angles;
        }

        public bool CheckAngles(double[] oldAngles, double[] angles)
        {
            for (int i = 0; i <= 5; i++)
            {
                if (oldAngles[i] != angles[i])
                    return false;
            }

            return true;
        }

        public double PartialGradient(Vector3D target, double[] angles, int i)
        {
            // Saves the angle,
            // it will be restored later
            double angle = angles[i];

            // Gradient : [F(x+SamplingDistance) - F(x)] / h
            double fX = DistanceFromTarget(target, angles);

            angles[i] += _samplingDistance;
            double fXPlusD = DistanceFromTarget(target, angles);

            double gradient = (fXPlusD - fX) / _samplingDistance;

            // Restores
            angles[i] = angle;

            return gradient;
        }


        public double DistanceFromTarget(Vector3D target, double[] angles)
        {
            Vector3D point = ForwardKinematics(angles);
            return Math.Sqrt(Math.Pow((point.X - target.X), 2.0) + Math.Pow((point.Y - target.Y), 2.0) +
                             Math.Pow((point.Z - target.Z), 2.0));
        }


        public Vector3D ForwardKinematics(double[] angles)
        {
            //The base only has rotation and is always at the origin, so the only transform in the transformGroup is the rotation R
            _f1 = new Transform3DGroup();
            _r =
                new
                    RotateTransform3D(new AxisAngleRotation3D(new Vector3D(_joints[0].RotAxisX, _joints[0].RotAxisY, _joints[0].RotAxisZ), angles[0]),
                                      new Point3D(_joints[0].RotPointX, _joints[0].RotPointY, _joints[0].RotPointZ));
            _f1.Children.Add(_r);

            //This moves the first joint attached to the base, it may translate and rotate. Since the joint are already in the right position (the .stl model also store the joints position
            //in the virtual world when they were first created, so if you load all the .stl models of the joint they will be automatically positioned in the right locations)
            //so in all of these cases the first translation is always 0, I just left it for future purposes if something need to be moved
            //After that, the joint needs to rotate of a certain amount (given by the value in the slider), and the rotation must be executed on a specific point
            //After some testing it looks like the point 175, -200, 500 is the sweet spot to achieve the rotation intended for the joint
            //finally we also need to apply the transformation applied to the base 
            _f2 = new Transform3DGroup();
            _  = new TranslateTransform3D(0, 0, 0);
            _r =
                new
                    RotateTransform3D(new AxisAngleRotation3D(new Vector3D(_joints[1].RotAxisX, _joints[1].RotAxisY, _joints[1].RotAxisZ), angles[1]),
                                      new Point3D(_joints[1].RotPointX, _joints[1].RotPointY, _joints[1].RotPointZ));
            _f2.Children.Add(_);
            _f2.Children.Add(_r);
            _f2.Children.Add(_f1);

            //The second joint is attached to the first one. As before I found the sweet spot after testing, and looks like is rotating just fine. No pre-translation as before
            //and again the previous transformation needs to be applied
            _f3 = new Transform3DGroup();
            _  = new TranslateTransform3D(0, 0, 0);
            _r =
                new
                    RotateTransform3D(new AxisAngleRotation3D(new Vector3D(_joints[2].RotAxisX, _joints[2].RotAxisY, _joints[2].RotAxisZ), angles[2]),
                                      new Point3D(_joints[2].RotPointX, _joints[2].RotPointY, _joints[2].RotPointZ));
            _f3.Children.Add(_);
            _f3.Children.Add(_r);
            _f3.Children.Add(_f2);

            //as before
            _f4 = new Transform3DGroup();
            _  = new TranslateTransform3D(0, 0, 0); //1500, 650, 1650
            _r =
                new
                    RotateTransform3D(new AxisAngleRotation3D(new Vector3D(_joints[3].RotAxisX, _joints[3].RotAxisY, _joints[3].RotAxisZ), angles[3]),
                                      new Point3D(_joints[3].RotPointX, _joints[3].RotPointY, _joints[3].RotPointZ));
            _f4.Children.Add(_);
            _f4.Children.Add(_r);
            _f4.Children.Add(_f3);

            //as before
            _f5 = new Transform3DGroup();
            _  = new TranslateTransform3D(0, 0, 0);
            _r =
                new
                    RotateTransform3D(new AxisAngleRotation3D(new Vector3D(_joints[4].RotAxisX, _joints[4].RotAxisY, _joints[4].RotAxisZ), angles[4]),
                                      new Point3D(_joints[4].RotPointX, _joints[4].RotPointY, _joints[4].RotPointZ));
            _f5.Children.Add(_);
            _f5.Children.Add(_r);
            _f5.Children.Add(_f4);

            //NB: I was having a nightmare trying to understand why it was always rotating in a weird way... SO I realized that the order in which
            //you add the Children is actually VERY IMPORTANT in fact before I was applyting F and then T and R, but the previous transformation
            //Should always be applied as last (FORWARD Kinematics)
            _f6 = new Transform3DGroup();
            _  = new TranslateTransform3D(0, 0, 0);
            _r =
                new
                    RotateTransform3D(new AxisAngleRotation3D(new Vector3D(_joints[5].RotAxisX, _joints[5].RotAxisY, _joints[5].RotAxisZ), angles[5]),
                                      new Point3D(_joints[5].RotPointX, _joints[5].RotPointY, _joints[5].RotPointZ));
            _f6.Children.Add(_);
            _f6.Children.Add(_r);
            _f6.Children.Add(_f5);


            _joints[0].Model.Transform = _f1; //First joint
            _joints[1].Model.Transform = _f2; //Second joint (the "biceps")
            _joints[2].Model.Transform = _f3; //third joint (the "knee" or "elbow")
            _joints[3].Model.Transform = _f4; //the "forearm"
            _joints[4].Model.Transform = _f5; //the tool plate
            _joints[5].Model.Transform = _f6; //the tool

            Tx.Content      = _joints[5].Model.Bounds.Location.X;
            Ty.Content      = _joints[5].Model.Bounds.Location.Y;
            Tz.Content      = _joints[5].Model.Bounds.Location.Z;
            Tx_Copy.Content = _geom.Bounds.Location.X;
            Ty_Copy.Content = _geom.Bounds.Location.Y;
            Tz_Copy.Content = _geom.Bounds.Location.Z;

#if IRB6700
            joints[6].model.Transform = F1;
            joints[7].model.Transform = F1;
            joints[19].model.Transform = F1;
            joints[14].model.Transform = F1;

            joints[8].model.Transform = F2;
            joints[9].model.Transform = F2;

            joints[10].model.Transform = F3;
            joints[11].model.Transform = F3;
            joints[12].model.Transform = F3;
            joints[16].model.Transform = F3;

            joints[13].model.Transform = F4;
            joints[17].model.Transform = F4;
#else
            _joints[7].Model.Transform = _f1; //Cables

            _joints[8].Model.Transform = _f2; //Cables

            _joints[6].Model.Transform = _f3; //The ABB writing
            _joints[9].Model.Transform = _f3; //Cables
#endif

            return new Vector3D(_joints[5].Model.Bounds.Location.X, _joints[5].Model.Bounds.Location.Y,
                                _joints[5].Model.Bounds.Location.Z);
        }
    }
}