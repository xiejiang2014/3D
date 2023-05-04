using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Wpf3DCubeWindow
{
    /// <summary>
    /// A Window which contains a Cube which has 2D content on each side
    /// </summary>
    public partial class CubeWindow : Window
    {
        #region Classes

        /// <summary>
        /// ��λ
        /// </summary>
        private enum CubeSide
        {
            Front,
            Back,
            Left,
            Right,
            Top,
            Bottom
        }

        /// <summary>
        /// ����
        /// </summary>
        private enum Direction
        {
            None,
            North,
            East,
            South,
            West
        }

        /// <summary>
        /// A class which represents the quaternion rotation needed to display
        /// a given cube side based on the initially displayed side being the 'front'.
        ///
        /// һ����ʾ��Ԫ����ת���࣬����ת���������ʾ�Ĳ���Ϊ�����桱����ʾ��������������档
        /// </summary>
        private class CubeRotation
        {
            public CubeSide CubeSide { get; set; }

            /// <summary>
            /// ��Ԫ��(System.Windows.Media.Media3D.Quaternion)
            /// </summary>
            public Quaternion Quaternion { get; set; }
        }

        #endregion Classes

        
        //6����
        private static CubeRotation _cubeFrontSideFacing;
        private static CubeRotation _cubeBackSideFacing;
        private static CubeRotation _cubeLeftSideFacing;
        private static CubeRotation _cubeRightSideFacing;
        private static CubeRotation _cubeTopSideFacing;
        private static CubeRotation _cubeBottomSideFacing;

        /// <summary>
        /// �����ֵ�  ��һ��Ϊλ��,�ڶ���Ϊ����  �õ� CubeRotation ����
        /// </summary>
        private static Dictionary<CubeSide, Dictionary<Direction, CubeRotation>> _possibleRotationMatrix;
        

        

        /// <summary>
        /// ��ǰ��
        /// </summary>
        private CubeRotation _currentCubeRotation;

        Point _originalMousePosition;
        Boolean _isRotating;
        
        


        #region Constructor

        /// <summary>
        /// Initializes the <see cref="CubeWindow"/> class.
        /// </summary>
        static CubeWindow()
        {
            // Create a class for each side of the cube which contains the 
            // quaternion rotation needed to display the side based on the
            // intially displayed side being the 'front'.

            //Ϊ�������ÿһ�洴��һ���࣬���а������ڳ�ʼ��ʾ��һ��Ϊ�����桱����ʾ�����������Ԫ����ת��
            _cubeFrontSideFacing = new CubeRotation()
            {
                CubeSide = CubeSide.Front,
                Quaternion = new Quaternion(new Vector3D(0, 0, 1), 0)
            };

            _cubeBackSideFacing = new CubeRotation()
            {
                CubeSide = CubeSide.Back,
                Quaternion = new Quaternion(new Vector3D(0, 1, 0), 180)
            };

            _cubeLeftSideFacing = new CubeRotation()
            {
                CubeSide = CubeSide.Left,
                Quaternion = new Quaternion(new Vector3D(0, 1, 0), -90)
            };

            _cubeRightSideFacing = new CubeRotation()
            {
                CubeSide = CubeSide.Right,
                Quaternion = new Quaternion(new Vector3D(0, 1, 0), 90)
            };

            _cubeTopSideFacing = new CubeRotation()
            {
                CubeSide = CubeSide.Top,
                Quaternion = new Quaternion(new Vector3D(1, 0, 0), -90)
            };

            _cubeBottomSideFacing = new CubeRotation()
            {
                CubeSide = CubeSide.Bottom,
                Quaternion = new Quaternion(new Vector3D(1, 0, 0), 90)
            };


            // For each cube side that could be facing there are 4 possible directions (based on user dragging a mouse)
            // to rotate the cube.
            // Store this as a collection for easy use later on.
            // ���ڿ��������ÿ���������棬�� 4 �����ܵķ��򣨻����û��϶���꣩����ת�����塣����洢Ϊһ�����ϣ��Ա��Ժ�ʹ�á�

            _possibleRotationMatrix = new Dictionary<CubeSide, Dictionary<Direction, CubeRotation>>();

            Dictionary<Direction, CubeRotation> possibleRotations = new Dictionary<Direction, CubeRotation>(5);
            
            // 1. FRONT - First store all possible rotations from the cube front side facing
            possibleRotations[Direction.None] = _cubeFrontSideFacing;
            possibleRotations[Direction.West] = _cubeRightSideFacing;
            possibleRotations[Direction.East] = _cubeLeftSideFacing;
            possibleRotations[Direction.North] = _cubeBottomSideFacing;
            possibleRotations[Direction.South] = _cubeTopSideFacing;

            _possibleRotationMatrix[CubeSide.Front] = possibleRotations;

            // 2. BACK
            possibleRotations = new Dictionary<Direction, CubeRotation>(5);
            possibleRotations[Direction.None] = _cubeBackSideFacing;
            possibleRotations[Direction.West] = _cubeLeftSideFacing;
            possibleRotations[Direction.East] = _cubeRightSideFacing;
            possibleRotations[Direction.North] = _cubeBottomSideFacing;
            possibleRotations[Direction.South] = _cubeTopSideFacing;

            _possibleRotationMatrix[CubeSide.Back] = possibleRotations;

            // 3. LEFT
            possibleRotations = new Dictionary<Direction, CubeRotation>(5);
            possibleRotations[Direction.None] = _cubeLeftSideFacing;
            possibleRotations[Direction.West] = _cubeFrontSideFacing;
            possibleRotations[Direction.East] = _cubeBackSideFacing;
            possibleRotations[Direction.North] = _cubeBottomSideFacing;
            possibleRotations[Direction.South] = _cubeTopSideFacing;

            _possibleRotationMatrix[CubeSide.Left] = possibleRotations;

            // 4. RIGHT
            possibleRotations = new Dictionary<Direction, CubeRotation>(5);
            possibleRotations[Direction.None] = _cubeRightSideFacing;
            possibleRotations[Direction.West] = _cubeBackSideFacing;
            possibleRotations[Direction.East] = _cubeFrontSideFacing;
            possibleRotations[Direction.North] = _cubeBottomSideFacing;
            possibleRotations[Direction.South] = _cubeTopSideFacing;

            _possibleRotationMatrix[CubeSide.Right] = possibleRotations;

            // 5. TOP
            possibleRotations = new Dictionary<Direction, CubeRotation>(5);
            possibleRotations[Direction.None] = _cubeTopSideFacing;
            possibleRotations[Direction.West] = _cubeRightSideFacing;
            possibleRotations[Direction.East] = _cubeLeftSideFacing;
            possibleRotations[Direction.North] = _cubeFrontSideFacing;
            possibleRotations[Direction.South] = _cubeBackSideFacing;

            _possibleRotationMatrix[CubeSide.Top] = possibleRotations;

            // 6. BOTTOM
            possibleRotations = new Dictionary<Direction, CubeRotation>(5);
            possibleRotations[Direction.None] = _cubeBottomSideFacing;
            possibleRotations[Direction.West] = _cubeRightSideFacing;
            possibleRotations[Direction.East] = _cubeLeftSideFacing;
            possibleRotations[Direction.North] = _cubeBackSideFacing;
            possibleRotations[Direction.South] = _cubeFrontSideFacing;

            _possibleRotationMatrix[CubeSide.Bottom] = possibleRotations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CubeWindow"/> class.
        /// </summary>
        public CubeWindow()
        {
            InitializeComponent();
            _currentCubeRotation = _cubeFrontSideFacing;
        }

        #endregion Constructor


        #region Methods

        /// <summary>
        /// Handles the Click event of the CloseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Handles the PreviewMouseRightButtonDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the original mouse position as the user starts dragging...
            //�Ϸŵ���ʼ��
            _originalMousePosition = e.GetPosition(this.CubeV3D);
        }

        /// <summary>
        /// Handles the MouseRightButtonUp event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Get the finishing mouse position (when the user has finished dragging)
            //�Ϸŵ���ֹ��
            Point newPosition = e.GetPosition(this.CubeV3D);

            Double deltaX = newPosition.X - _originalMousePosition.X;
            Double deltaY = newPosition.Y - _originalMousePosition.Y;

            // Don't bother doing anything if the user has hardly moved the mouse...
            //����û�����û���ƶ���꣬�벻Ҫ�������κ�����......
            if (Math.Abs(deltaX) <= SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(deltaY) <= SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            // A vector defines a length and direction

            Vector vectorY = new Vector(0, deltaY);
            Vector vectorX = new Vector(deltaX, 0);

            // Add the X and Y vectors together to get the actual vector representing the mouse movement
            // (although we are not actually interested in the 'length' part of the vector)
            Vector vectorXPlusY = vectorX + vectorY;

            // Now get the angle between due east and this vector
            Double angleBetween = Vector.AngleBetween(new Vector(newPosition.X, 0), vectorXPlusY);
            Direction directionToRotate;

            if (angleBetween > -135 && angleBetween < -45)
            {
                directionToRotate = Direction.North;
            }
            else if (angleBetween >= -45 && angleBetween < 45)
            {
                directionToRotate = Direction.East;
            }
            else if (angleBetween >= 45 && angleBetween < 135)
            {
                directionToRotate = Direction.South;
            }
            else
            {
                directionToRotate = Direction.West;
            }

            //���˵õ�����ת�ķ���
            RotateCube(directionToRotate);
        }

        /// <summary>
        /// Toggles visual hit testing on and off.
        /// </summary>
        /// <param name="isOn">if set to <c>true</c> [is on].</param>
        private void ToggleVisualHitTesting(bool isOn)
        {
            this.IsHitTestVisible = isOn;
        }

        /// <summary>
        /// Rotates the cube (gives the illusion by rotating the camera).
        /// </summary>
        /// <param name="direction">The direction.</param>
        private void RotateCube(Direction direction)
        {
            if (!_isRotating)
            {
                // Let the quaternion animation figure out how to transform between 2 quaternions...
                QuaternionAnimation animation = new QuaternionAnimation();

                // A quaternion is way of representing a rotation around an axis

                // The From quaternion is the one required to display the current cube side based on the original side being the 'front'  
                animation.From = _possibleRotationMatrix[_currentCubeRotation.CubeSide][Direction.None].Quaternion;
                // The To quaternion is the one required to display the next cube side based on the original side being the 'front'
                animation.To = _possibleRotationMatrix[_currentCubeRotation.CubeSide][direction].Quaternion;

                animation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 650));

                _isRotating = true;
                
                animation.Completed += (o, e) =>
                {
                    _isRotating = false;
                    //�������������¿�ʼ���в���
                    ToggleVisualHitTesting(true);

                    //��¼��ǰ��
                    _currentCubeRotation = _possibleRotationMatrix[_currentCubeRotation.CubeSide][direction];
                };

                // Temporarily remove hit testing to make things a but smoother
                //����תʱ�������в���
                ToggleVisualHitTesting(false);

                //��ʼת������
                this.CameraRotation.BeginAnimation(QuaternionRotation3D.QuaternionProperty, animation);
            }
        }

        #endregion Methods
    }
}
