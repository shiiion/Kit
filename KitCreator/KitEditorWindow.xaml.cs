using System;
using System.Windows;
using Kit.Graphics.Components;
using Kit.Graphics.Types;
using Kit.Graphics.Drawing;
using System.Windows.Media;

namespace KitCreator
{
    /// <summary>
    /// Interaction logic for KitEditorWindow.xaml
    /// </summary>
    public partial class KitEditorWindow : Window
    {
        public TopLevelComponent TopLevelComponent { get; set; }

        private KitBrush componentBrush;

        private Type selectedClassType;

        public KitEditorWindow()
        {
            InitializeComponent();
            TopLevelComponent = new TopLevelComponent(new Vector2(300, 300), "Kit", () => { }, "", "")
            {
                OpacityMode = false,
                Location = new Vector2(200, 20)
            };
            TopLevelComponent.TitleBar.Opacity = 1;
            componentBrush = new KitBrush();

            selectedClassType = new KitComponent().GetType();

            CompositionTarget.Rendering += (o, e) =>
            {
                if (TopLevelComponent.Redraw)
                {
                    InvalidateVisual();
                }
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            BeginPaint(drawingContext);
            base.OnRender(drawingContext);
        }

        public void BeginPaint(DrawingContext context)
        {
            componentBrush.InitRender(context);
            componentBrush.DrawRoundedRectangle(new Box(0, 0, 144, Height - 39), true, Colors.White, 5, 5);
            TopLevelComponent.PreDrawComponentTree(componentBrush);
            TopLevelComponent.DrawComponentTree(componentBrush, false);
        }

        private void KitComponent_Selected(object sender, RoutedEventArgs e)
        {
            selectedClassType = new KitComponent().GetType();
        }

        private void KitImage_Selected(object sender, RoutedEventArgs e)
        {
            selectedClassType = new KitImage().GetType();
        }

        private void KitBox_Selected(object sender, RoutedEventArgs e)
        {
            selectedClassType = new KitBox(Colors.Black, new Vector2()).GetType();
        }

        private void KitText_Selected(object sender, RoutedEventArgs e)
        {
            selectedClassType = new KitText().GetType();
        }

        private void KitRevealText_Selected(object sender, RoutedEventArgs e)
        {
            selectedClassType = new KitRevealText(0, "", "", 0).GetType();
        }

        private void KitScrollbar_Selected(object sender, RoutedEventArgs e)
        {
            selectedClassType = new KitComponent().GetType();
        }

        private void KitTextAnimation_Selected(object sender, RoutedEventArgs e)
        {
            selectedClassType = new KitComponent().GetType();
        }

        private void KitTextArea_Selected(object sender, RoutedEventArgs e)
        {
            selectedClassType = new KitComponent().GetType();
        }

        private void KitTextBox_Selected(object sender, RoutedEventArgs e)
        {
            selectedClassType = new KitComponent().GetType();
        }
    }
}
