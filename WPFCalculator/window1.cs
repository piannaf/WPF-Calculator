using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; //for binding
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Piannaf.Ports.Microsoft.Samples.WPF.Calculator
{
    public class Window1 : Window
    {
        static MyTextBox DisplayBox; // MyTextBox defined in mytextbox.cs
        static MyTextBox PaperBox;
        static PaperTrail Paper;

        //declare objects written in XAML needed by other methods
        static DockPanel MyPanel;
        static Grid MyGrid;
        TextBlock BMemBox;
        MenuItem StandardMenu;

        #region Constructor
        public Window1()
            : base()
        {
            this.InitializeThis();

            DisplayBox = new MyTextBox();
            Grid.SetRow(DisplayBox, 0);
            Grid.SetColumn(DisplayBox, 0);
            Grid.SetColumnSpan(DisplayBox, 9);
            DisplayBox.Height = 30;
            MyGrid.Children.Add(DisplayBox); //MyGrid defined in XAML [InitializeThis]

            //sub-class our paper trail textBox
            PaperBox = new MyTextBox();
            Grid.SetRow(PaperBox, 1);
            Grid.SetColumn(PaperBox, 0);
            Grid.SetColumnSpan(PaperBox, 3);
            Grid.SetRowSpan(PaperBox, 5);
            PaperBox.IsReadOnly = true;
            PaperBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            PaperBox.Margin = new Thickness(3.0, 1.0, 1.0, 1.0);
            PaperBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            Paper = new PaperTrail();

            MyGrid.Children.Add(PaperBox);

            ProcessKey('0'); //force initial state to 0.
            EraseDisplay = true; //human sees 0, machine will overwrite on next key
        }

        void InitializeThis()
        {
            #region Window Properties
            /*<Window x:Class="WPFCalculator.Window1"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                Title="WPF Calculator"
                Height="400"
                Width="600" 
                ResizeMode="CanMinimize"  
                Icon="AppIcon.ico"
                TextInput="OnWindowKeyDown"
               >*/
            this.Title = "WPF Calculator";
            this.Height = 400;
            this.Width = 600;
            this.ResizeMode = ResizeMode.CanMinimize;
            //http://msdn.microsoft.com/en-us/library/system.windows.window.icon.aspx
            Uri iconUri = new Uri("appicon.ico", UriKind.Relative);
            this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
            this.TextInput += new System.Windows.Input.TextCompositionEventHandler(OnWindowKeyDown);
            NameScope.SetNameScope(this, new NameScope()); //for storyboard
            #endregion

            #region Member Definitions
            //<DockPanel Name="MyPanel">
            MyPanel = new DockPanel();
            MyPanel.Name = "MyPanel";

            //<Grid Name="MyGrid" Background="Wheat"  ShowGridLines="False">
            MyGrid = new Grid();
            MyGrid.Name = "MyGrid";
            MyGrid.Background = Brushes.Wheat;
            MyGrid.ShowGridLines = false;
            #endregion

            //<Grid.Resources >
            #region Animation
            //<Storyboard x:Key="playStoryboard">
            Storyboard playStoryboard = new Storyboard();
            this.RegisterName("playStoryboard", playStoryboard);
            //  <DoubleAnimation From="50"  To="40" Duration="0:0:0.25" RepeatBehavior="1x" AutoReverse="True" 
            //      Storyboard.TargetName="TB" Storyboard.TargetProperty="(Ellipse.Height)"/>
            DoubleAnimation TBHeightAnimation = new DoubleAnimation(50.0, 40.0, new Duration(new TimeSpan(0, 0, 0, 0, 250))); //guess
            TBHeightAnimation.RepeatBehavior = new RepeatBehavior(1.0); //1x gives the error "cannot implicitly convert int to RepeatBehaviour"
            TBHeightAnimation.AutoReverse = true;
            Storyboard.SetTargetName(TBHeightAnimation, "TB");
            Storyboard.SetTargetProperty(TBHeightAnimation, new PropertyPath(Ellipse.HeightProperty));
            playStoryboard.Children.Add(TBHeightAnimation);

            //  <DoubleAnimation From="50"  To="44" Duration="0:0:0.25" RepeatBehavior="1x" AutoReverse="True" 
            //      Storyboard.TargetName="TB" Storyboard.TargetProperty="(Ellipse.Width)"/>
            DoubleAnimation TBWidthAnimation = new DoubleAnimation(50.0, 44.0, new Duration(new TimeSpan(0, 0, 0, 0, 250)));
            TBWidthAnimation.RepeatBehavior = new RepeatBehavior(1.0);
            TBWidthAnimation.AutoReverse = true;
            Storyboard.SetTargetName(TBWidthAnimation, "TB");
            Storyboard.SetTargetProperty(TBWidthAnimation, new PropertyPath(Ellipse.WidthProperty));
            playStoryboard.Children.Add(TBWidthAnimation);
            //</Storyboard>
            MyGrid.Resources.Add("playStoryboard", playStoryboard);
            /* The animations make the ellipses become circles... (including Microsoft's) */
            #endregion

            #region Style
            //<Style x:Key="DigitBtn"  TargetType="{x:Type Button}">
            //http://msdn.microsoft.com/en-us/library/ms753322.aspx
            Style DigitBtn = new Style();
            DigitBtn.TargetType = typeof(Button);

            //  <Setter Property="Focusable" Value="False"/>
            //http://msdn.microsoft.com/en-us/library/ms587945.aspx
            //http://blogsprajeesh.blogspot.com/2009/03/wpf-data-binding-datatriggers.html
            Setter setter1 = new Setter(Button.FocusableProperty, false);
            DigitBtn.Setters.Add(setter1);

            //  <Setter Property="FontSize" Value="14pt"/>
            //http://stackoverflow.com/questions/1279102/how-do-you-set-the-frameworkelement-width-property-to-the-value-of-a-qualifieddou
            LengthConverter lc = new LengthConverter();
            string qualifiedDouble = "14pt";
            Setter setter2 = new Setter(Button.FontSizeProperty, lc.ConvertFrom(qualifiedDouble));
            DigitBtn.Setters.Add(setter2);

            //  <Setter Property="Margin" Value="0"/>
            // http://msdn.microsoft.com/en-us/library/system.windows.frameworkelement.margin.aspx
            Setter setter3 = new Setter(Button.MarginProperty, new Thickness(0));
            DigitBtn.Setters.Add(setter3);

            //  <Setter Property="Template">
            Setter setter4 = new Setter();
            setter4.Property = Button.TemplateProperty;
            //    <Setter.Value>
            //      <ControlTemplate TargetType="{x:Type Button}">
            ControlTemplate ct = new ControlTemplate(typeof(Button));
            //        <Grid Width="60" Height="50">
            //http://www.vistax64.com/avalon/23416-changing-drawing-style-button-code-c-c.html
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
            //getting error with integer literal,
            //needs to be double http://msdn.microsoft.com/en-us/library/system.windows.frameworkelement.width.aspx
            grid.SetValue(Grid.WidthProperty, 60.0); 
            grid.SetValue(Grid.HeightProperty, 50.0);
            //              <Ellipse Width="57" Height="49" x:Name="TB"  StrokeThickness="1"
            //                   Stroke="{TemplateBinding Foreground}" Fill="{TemplateBinding Background}"
            //                   HorizontalAlignment="Center" VerticalAlignment="Center" />
            FrameworkElementFactory TB = new FrameworkElementFactory(typeof(Ellipse), "TB");
            TB.SetValue(Ellipse.WidthProperty, 57.0);
            TB.SetValue(Ellipse.HeightProperty, 49.0);
            //Stroke is also double http://msdn.microsoft.com/en-us/library/system.windows.shapes.shape.strokethickness.aspx
            TB.SetValue(Ellipse.StrokeThicknessProperty, 1.0);
            //See ContentPresenter for how to make the binding
            TB.SetValue(Ellipse.StrokeProperty, new Binding("Foreground") { RelativeSource = RelativeSource.TemplatedParent, Mode = BindingMode.OneWay });
            TB.SetValue(Ellipse.FillProperty, new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent, Mode = BindingMode.OneWay });
            TB.SetValue(Ellipse.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            TB.SetValue(Ellipse.VerticalAlignmentProperty, VerticalAlignment.Center);
            this.RegisterName("TB", TB); //for use in storyboard

            //          <ContentPresenter Content="{TemplateBinding Content}" HorizontalAlignment="Center" 
            //            VerticalAlignment="Center"/>
            //TODO: research http://msdn.microsoft.com/en-us/library/system.windows.frameworkelement.aspx
            FrameworkElementFactory CP = new FrameworkElementFactory(typeof(ContentPresenter));
            //http://msdn.microsoft.com/en-us/library/ms742882.aspx
            //http://msdn.microsoft.com/en-us/library/ms742863.aspx
            Binding TBinding = new Binding("Content"){RelativeSource = RelativeSource.TemplatedParent, Mode = BindingMode.OneWay};
            CP.SetValue(ContentPresenter.ContentProperty, TBinding);
            CP.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            CP.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            //        </Grid>
            grid.AppendChild(TB);
            grid.AppendChild(CP);

          //<ControlTemplate.Triggers>
          //    <Trigger Property="IsMouseOver" Value="true">
            Trigger IMO = new Trigger();
            IMO.Property = Button.IsMouseOverProperty; //guess
            IMO.Value = true;
          //        <Setter TargetName="TB" Property="Ellipse.Fill" Value="Lightblue" />
            Setter IMOSetter = new Setter(Ellipse.FillProperty, Brushes.LightBlue, "TB");
            IMO.Setters.Add(IMOSetter);
          //    </Trigger>
            ct.Triggers.Add(IMO);

          //    <Trigger Property="IsPressed" Value="true">
            Trigger IP = new Trigger();
            IP.Property = Button.IsPressedProperty;
            IP.Value = true;
          //        <Setter TargetName="TB" Property="Ellipse.Fill" Value="Blue" />
            Setter IPSetter = new Setter(Ellipse.FillProperty, Brushes.Blue, "TB");
            IP.Setters.Add(IPSetter);
          //    </Trigger>
            ct.Triggers.Add(IP);

          //    <EventTrigger RoutedEvent="ButtonBase.Click">
            //http://msdn.microsoft.com/en-us/library/system.windows.controls.primitives.buttonbase.aspx
            EventTrigger BBC = new EventTrigger(System.Windows.Controls.Primitives.ButtonBase.ClickEvent);
          //        <EventTrigger.Actions>
          //            <BeginStoryboard Name="playStoryboard" Storyboard="{StaticResource playStoryboard}"/>
            BeginStoryboard BsPs = new BeginStoryboard();
            BsPs.Storyboard = (Storyboard)MyGrid.Resources["playStoryboard"];
            BsPs.Name = "playStoryboard";
          //        </EventTrigger.Actions>
            BBC.Actions.Add(BsPs);
          //    </EventTrigger>
            ct.Triggers.Add(BBC);
          //</ControlTemplate.Triggers>

            //      </ControlTemplate>
            ct.VisualTree = grid;

            //    </Setter.Value>
            setter4.Value = ct;
            //  </Setter>
            DigitBtn.Setters.Add(setter4);
            //</Style>
            MyGrid.Resources.Add("DigitBtn", DigitBtn);
            #endregion
            //</Grid.Resources>

            #region Grid Definitions
            //<Grid.ColumnDefinitions>
            //    <ColumnDefinition/>
            MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //    <ColumnDefinition/>
            MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //    <ColumnDefinition/>
            MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //    <ColumnDefinition/>
            MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //    <ColumnDefinition/>
            MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //    <ColumnDefinition/>
            MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //    <ColumnDefinition/>
            MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //    <ColumnDefinition/>
            MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //    <ColumnDefinition/>
            MyGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //</Grid.ColumnDefinitions>
            //<Grid.RowDefinitions>
            //    <RowDefinition/>
            MyGrid.RowDefinitions.Add(new RowDefinition());
            //    <RowDefinition/>
            MyGrid.RowDefinitions.Add(new RowDefinition());
            //    <RowDefinition/>
            MyGrid.RowDefinitions.Add(new RowDefinition());
            //    <RowDefinition/>
            MyGrid.RowDefinitions.Add(new RowDefinition());
            //    <RowDefinition/>
            MyGrid.RowDefinitions.Add(new RowDefinition());
            //    <RowDefinition/>
            MyGrid.RowDefinitions.Add(new RowDefinition());
            //</Grid.RowDefinitions>
            #endregion

            #region Buttons
            //<Button Name="B7" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="4" Grid.Row="2">7</Button>
          //<Button Name="B8" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="5" Grid.Row="2">8</Button>
          //<Button Name="B9" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="6" Grid.Row="2">9</Button>
            Button B7 = new Button();
            B7.Name = "B7";
            B7.Click += new RoutedEventHandler(DigitBtn_Click);
            B7.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B7, 4);
            Grid.SetRow(B7, 2);
            //http://www.daniweb.com/forums/thread196582.html
            //look at http://msdn.microsoft.com/en-us/library/system.windows.controls.button.aspx
            //not http://msdn.microsoft.com/en-us/library/system.web.ui.webcontrols.button(VS.100).aspx
            B7.Content = "7";
            MyGrid.Children.Add(B7);

            Button B8 = new Button();
            B8.Name = "B8";
            B8.Click += new RoutedEventHandler(DigitBtn_Click);
            //http://msdn.microsoft.com/en-us/library/cscsdfbt(VS.71).aspx
            B8.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B8, 5);
            Grid.SetRow(B8, 2);
            B8.Content = "8";
            MyGrid.Children.Add(B8);

            Button B9 = new Button();
            B9.Name = "B9";
            B9.Click += new RoutedEventHandler(DigitBtn_Click);
            B9.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B9, 6);
            Grid.SetRow(B9, 2);
            B9.Content = "9";
            MyGrid.Children.Add(B9);

          //<Button Name="B4" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="4" Grid.Row="3" >4</Button>
          //<Button Name="B5" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="5" Grid.Row="3" >5</Button>
          //<Button Name="B6" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="6" Grid.Row="3" >6</Button>
            Button B4 = new Button();
            B4.Name = "B4";
            B4.Click += new RoutedEventHandler(DigitBtn_Click);
            B4.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B4, 4);
            Grid.SetRow(B4, 3);
            B4.Content = "4";
            MyGrid.Children.Add(B4);

            Button B5 = new Button();
            B5.Name = "B5";
            B5.Click += new RoutedEventHandler(DigitBtn_Click);
            B5.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B5, 5);
            Grid.SetRow(B5, 3);
            B5.Content = "5";
            MyGrid.Children.Add(B5);

            Button B6 = new Button();
            B6.Name = "B6";
            B6.Click += new RoutedEventHandler(DigitBtn_Click);
            B6.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B6, 6);
            Grid.SetRow(B6, 3);
            B6.Content = "6";
            MyGrid.Children.Add(B6);


          //<Button Name="B1" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="4" Grid.Row="4" >1</Button>
          //<Button Name="B2" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="5" Grid.Row="4" >2</Button>
          //<Button Name="B3" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="6" Grid.Row="4" >3</Button>
            Button B1 = new Button();
            B1.Name = "B1";
            B1.Click += new RoutedEventHandler(DigitBtn_Click);
            B1.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B1, 4);
            Grid.SetRow(B1, 4);
            B1.Content = "1";
            MyGrid.Children.Add(B1);

            Button B2 = new Button();
            B2.Name = "B2";
            B2.Click += new RoutedEventHandler(DigitBtn_Click);
            B2.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B2, 5);
            Grid.SetRow(B2, 4);
            B2.Content = "2";
            MyGrid.Children.Add(B2);

            Button B3 = new Button();
            B3.Name = "B3";
            B3.Click += new RoutedEventHandler(DigitBtn_Click);
            B3.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B3, 6);
            Grid.SetRow(B3, 4);
            B3.Content = "3";
            MyGrid.Children.Add(B3);

            //<Button Name="B0" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="4" Grid.Row="5" >0</Button>
            Button B0 = new Button();
            B0.Name = "B0";
            B0.Click += new RoutedEventHandler(DigitBtn_Click);
            B0.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(B0, 4);
            Grid.SetRow(B0, 5);
            B0.Content = "0";
            MyGrid.Children.Add(B0);

            //<Button Name="BPeriod" Click="DigitBtn_Click" Style="{StaticResource DigitBtn}" Grid.Column="5" Grid.Row="5" >.</Button>
            Button BPeriod = new Button();
            BPeriod.Name = "BPeriod";
            BPeriod.Click += new RoutedEventHandler(DigitBtn_Click);
            BPeriod.Style = MyGrid.Resources["DigitBtn"] as Style;
            Grid.SetColumn(BPeriod, 5);
            Grid.SetRow(BPeriod, 5);
            BPeriod.Content = ".";
            MyGrid.Children.Add(BPeriod);

            //<Button Name="BPM"        Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="6" Grid.Row="5" >+/-</Button>
            Button BPM = new Button();
            BPM.Name = "BPM";
            BPM.Click += new RoutedEventHandler(OperBtn_Click);
            BPM.Background = Brushes.DarkGray;
            BPM.Style = DigitBtn; //try without resources
            Grid.SetColumn(BPM, 6);
            Grid.SetRow(BPM, 5);
            BPM.Content = "+/-";
            MyGrid.Children.Add(BPM);

            //<Button Name="BDevide"    Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}" Grid.Column="7" Grid.Row="2" >/</Button>
            Button BDevide = new Button();
            BDevide.Name = "BDevide";
            BDevide.Click += new RoutedEventHandler(OperBtn_Click);
            BDevide.Background = Brushes.DarkGray;
            BDevide.Style = DigitBtn;
            Grid.SetColumn(BDevide, 7);
            Grid.SetRow(BDevide, 2);
            BDevide.Content = "/";
            MyGrid.Children.Add(BDevide);

            //<Button Name="BMultiply"  Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="7" Grid.Row="3" >*</Button>
            Button BMultiply = new Button();
            BMultiply.Name = "BMultiply";
            BMultiply.Click += new RoutedEventHandler(OperBtn_Click);
            BMultiply.Background = Brushes.DarkGray;
            BMultiply.Style = DigitBtn;
            Grid.SetColumn(BMultiply, 7);
            Grid.SetRow(BMultiply, 3);
            BMultiply.Content = "*";
            MyGrid.Children.Add(BMultiply);

            //<Button Name="BMinus" 	Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="7" Grid.Row="4" >-</Button>
            Button BMinus = new Button();
            BMinus.Name = "BMinus";
            BMinus.Click += new RoutedEventHandler(OperBtn_Click);
            BMinus.Background = Brushes.DarkGray;
            BMinus.Style = DigitBtn;
            Grid.SetColumn(BMinus, 7);
            Grid.SetRow(BMinus, 4);
            BMinus.Content = "-";
            MyGrid.Children.Add(BMinus);

            //<Button Name="BPlus"      Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="7" Grid.Row="5" >+</Button>
            Button BPlus = new Button();
            BPlus.Name = "BPlus";
            BPlus.Click += new RoutedEventHandler(OperBtn_Click);
            BPlus.Background = Brushes.DarkGray;
            BPlus.Style = DigitBtn;
            Grid.SetColumn(BPlus, 7);
            Grid.SetRow(BPlus, 5);
            BPlus.Content = "+";
            MyGrid.Children.Add(BPlus);

          


            //<Button Name="BSqrt" 		 Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="8" Grid.Row="2"   ToolTip="Usage: 'A Sqrt'" >Sqrt</Button>
            Button BSqrt = new Button();
            BSqrt.Name = "BSqrt";
            BSqrt.Click += new RoutedEventHandler(OperBtn_Click);
            BSqrt.Background = Brushes.DarkGray;
            BSqrt.Style = DigitBtn;
            Grid.SetColumn(BSqrt, 8);
            Grid.SetRow(BSqrt, 2);
            BSqrt.ToolTip = "Usage: 'A Sqrt'";
            BSqrt.Content = "Sqrt";
            MyGrid.Children.Add(BSqrt);

            //<Button Name="BPercent" 	 Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="8" Grid.Row="3"   ToolTip="Usage: 'A % B ='" >%</Button>
            Button BPercent = new Button();
            BPercent.Name = "BPercent";
            BPercent.Click += new RoutedEventHandler(OperBtn_Click);
            BPercent.Background = Brushes.DarkGray;
            BPercent.Style = DigitBtn;
            Grid.SetColumn(BPercent, 8);
            Grid.SetRow(BPercent, 3);
            BPercent.ToolTip = "Usage: 'A % B ='";
            BPercent.Content = "%";
            MyGrid.Children.Add(BPercent);

            //<Button Name="BOneOver" 	 Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="8" Grid.Row="4"   ToolTip="Usage: 'A 1/X'">1/X</Button>
            Button BOneOver = new Button();
            BOneOver.Name = "BOneOver";
            BOneOver.Click += new RoutedEventHandler(OperBtn_Click);
            BOneOver.Background = Brushes.DarkGray;
            BOneOver.Style = DigitBtn;
            Grid.SetColumn(BOneOver, 8);
            Grid.SetRow(BOneOver, 4);
            BOneOver.ToolTip = "Usage: 'A 1/X'";
            BOneOver.Content = "1/X";
            MyGrid.Children.Add(BOneOver);

            //<Button Name="BEqual" 	 Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="8" Grid.Row="5" >=</Button>
            Button BEqual = new Button();
            BEqual.Name = "BEqual";
            BEqual.Click += new RoutedEventHandler(OperBtn_Click);
            BEqual.Background = Brushes.DarkGray;
            BEqual.Style = DigitBtn;
            Grid.SetColumn(BEqual, 8);
            Grid.SetRow(BEqual, 5);
            BEqual.Content = "=";
            MyGrid.Children.Add(BEqual);

          

            //<Button Name="BC"  Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="8" Grid.Row="1" Grid.ColumnSpan="1" ToolTip="Clear All">C</Button>
            Button BC = new Button();
            BC.Name = "BC";
            BC.Click += new RoutedEventHandler(OperBtn_Click);
            BC.Background = Brushes.DarkGray;
            BC.Style = DigitBtn;
            Grid.SetColumn(BC, 8);
            Grid.SetRow(BC, 1);
            BC.ToolTip = "Clear All";
            BC.Content = "C";
            MyGrid.Children.Add(BC);

            //<Button Name="BCE" Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="7" Grid.Row="1" Grid.ColumnSpan="1"  ToolTip="Clear Current Entry">CE</Button>
            Button BCE = new Button();
            BCE.Name = "BCE";
            BCE.Click += new RoutedEventHandler(OperBtn_Click);
            BCE.Background = Brushes.DarkGray;
            BCE.Style = DigitBtn;
            Grid.SetColumn(BCE, 7);
            Grid.SetRow(BCE, 1);
            BCE.ToolTip = "Clear Current Entry";
            BCE.Content = "CE";
            MyGrid.Children.Add(BCE);

          
        
            //<Button Name="BMemClear" 	  Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="3" Grid.Row="2"  ToolTip="Clear Memory" >MC</Button>
            Button BMemClear = new Button();
            BMemClear.Name = "BMemClear";
            BMemClear.Click += new RoutedEventHandler(OperBtn_Click);
            BMemClear.Background = Brushes.DarkGray;
            BMemClear.Style = DigitBtn;
            Grid.SetColumn(BMemClear, 3);
            Grid.SetRow(BMemClear, 2);
            BMemClear.ToolTip = "Clear Memory";
            BMemClear.Content = "MC";
            MyGrid.Children.Add(BMemClear);

            //<Button Name="BMemRecall"   Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="3" Grid.Row="3"  ToolTip="Recall Memory">MR</Button>
            Button BMemRecall = new Button();
            BMemRecall.Name = "BMemRecall";
            BMemRecall.Click += new RoutedEventHandler(OperBtn_Click);
            BMemRecall.Background = Brushes.DarkGray;
            BMemRecall.Style = DigitBtn;
            Grid.SetColumn(BMemRecall, 3);
            Grid.SetRow(BMemRecall, 3);
            BMemRecall.ToolTip = "Recall Memory";
            BMemRecall.Content = "MR";
            MyGrid.Children.Add(BMemRecall);

            //<Button Name="BMemSave" 	  Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="3" Grid.Row="4"  ToolTip="Store in Memory">MS</Button>
            Button BMemSave = new Button();
            BMemSave.Name = "BMemSave";
            BMemSave.Click += new RoutedEventHandler(OperBtn_Click);
            BMemSave.Background = Brushes.DarkGray;
            BMemSave.Style = DigitBtn;
            Grid.SetColumn(BMemSave, 3);
            Grid.SetRow(BMemSave, 4);
            BMemSave.ToolTip = "Store in Memory";
            BMemSave.Content = "MS";
            MyGrid.Children.Add(BMemSave);

            //<Button Name="BMemPlus" 	  Click="OperBtn_Click" Background="Darkgray" Style="{StaticResource DigitBtn}"  Grid.Column="3" Grid.Row="5"  ToolTip="Add To Memory">M+</Button>
            Button BMemPlus = new Button();
            BMemPlus.Name = "BMemPlus";
            BMemPlus.Click += new RoutedEventHandler(OperBtn_Click);
            BMemPlus.Background = Brushes.DarkGray;
            BMemPlus.Style = DigitBtn;
            Grid.SetColumn(BMemPlus, 3);
            Grid.SetRow(BMemPlus, 5);
            BMemPlus.ToolTip = "Add To Memory";
            BMemPlus.Content = "M+";
            MyGrid.Children.Add(BMemPlus);
            #endregion

            #region Memory Box
            //<TextBlock  Name="BMemBox"	Grid.Column="3" Grid.Row="1" Margin="10,17,10,17" Grid.ColumnSpan="2">Memory: [empty]</TextBlock>
            BMemBox = new TextBlock();
            BMemBox.Name = "BMemBox";
            Grid.SetColumn(BMemBox, 3);
            Grid.SetRow(BMemBox, 1);
            BMemBox.Margin = new Thickness(10.0, 17.0, 10.0, 17.0);
            Grid.SetColumnSpan(BMemBox, 2);
            BMemBox.Text = "Memory: [empty]";
            MyGrid.Children.Add(BMemBox);
            #endregion

            #region Menu
            //<Menu  DockPanel.Dock="Top" Height="26">
            Menu menu = new Menu();
            //http://msdn.microsoft.com/en-us/library/system.windows.controls.dockpanel.dock.aspx
            DockPanel.SetDock(menu, Dock.Top);
            menu.Height = 26;

            //  <MenuItem Header="File">
            MenuItem file = new MenuItem();
            file.Header = "File";

            //    <MenuItem Click="OnMenuExit" Header="Exit"/>
            MenuItem exit = new MenuItem();
            exit.Click += new RoutedEventHandler(OnMenuExit);
            exit.Header = "Exit";

            //  </MenuItem>
            //http://msdn.microsoft.com/en-us/library/system.windows.controls.menuitem.aspx
            file.Items.Add(exit);

            //  <MenuItem Header="View">
            MenuItem view = new MenuItem();
            view.Header = "View";

            //       <MenuItem Name="StandardMenu" Click="OnMenuStandard" IsCheckable="true" IsChecked="True" Header="Standard"/>
            StandardMenu = new MenuItem();
            StandardMenu.Name = "StandardMenu";
            StandardMenu.Click += new RoutedEventHandler(OnMenuStandard);
            StandardMenu.IsCheckable = true;
            StandardMenu.IsChecked = true;
            StandardMenu.Header = "Standard";

            //  </MenuItem>
            view.Items.Add(StandardMenu);

            //  <MenuItem Header="Help">
            MenuItem help = new MenuItem();
            help.Header = "help";

            //    <MenuItem  Click="OnMenuAbout" Header="About"/>
            MenuItem about = new MenuItem();
            about.Click += new RoutedEventHandler(OnMenuAbout);
            about.Header = "About";

            //  </MenuItem>
            help.Items.Add(about);

            //</Menu>
            menu.Items.Add(file);
            menu.Items.Add(view);
            menu.Items.Add(help);
            #endregion

            //</DockPanel>
            MyPanel.Children.Add(menu);
            MyPanel.Children.Add(MyGrid);

            //</Window>
            this.Content = MyPanel;
        }
        #endregion

        #region calc operation
        //copied from original
        private void OnWindowKeyDown(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            //string s = e.Text;
            //char c = (s.ToCharArray())[0];
            char c = e.Text[0];
            e.Handled = true;

            if ((c >= '0' && c <= '9') || c == '.' || c == '\b')  // '\b' is backspace
            {
                ProcessKey(c);
                return;
            }

            switch (c)
            {
                case '+':
                    ProcessOperation("BPlus");
                    break;
                case '-':
                    ProcessOperation("BMinus");
                    break;
                case '*':
                    ProcessOperation("BMultiply");
                    break;
                case '/':
                    ProcessOperation("BDevide");
                    break;
                case '%':
                    ProcessOperation("BPercent");
                    break;
                case '=':
                    ProcessOperation("BEqual");
                    break;
            }
        }

        private void ProcessKey(char c)
        {
            if (EraseDisplay) //EraseDisplay is a property
            {
                Display = string.Empty; //Display is a property
                EraseDisplay = false;
            }
            AddToDisplay(c);
        }

        #region display
        //flag to erase or just add to current display flag
        private bool _erasediplay; //Get and set through the property
        private bool EraseDisplay
        {
            get
            {
                return _erasediplay;

            }
            set
            {
                _erasediplay = value;
            }
        }

        //The current Calculator display
        private string _display; //Get and set through the property
        private string Display
        {
            get
            {
                return _display;
            }
            set
            {
                _display = value;
            }
        }

        private void AddToDisplay(char c)
        {
            if (c == '.')
            {
                if (Display.IndexOf('.', 0) >= 0) //decimal point already displayed
                    return;
                Display = Display + c;
            }
            else
            {
                if (c >= '0' && c <= '9')
                {
                    Display = Display + c;
                }
                else
                    if (c == '\b') //backspace
                    {
                        if (Display.Length <= 1)
                            Display = String.Empty;
                        else
                        {
                            int i = Display.Length;
                            Display = Display.Remove(i - 1, 1); //remove last char
                        }
                    }
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (Display == String.Empty) //String.Empty vs Display.Length == 0
                DisplayBox.Text = "0"; //DisplayBox is of type MyTextBox initialized in the constructor
            else
                DisplayBox.Text = Display;
        }
        #endregion

        #region button callbacks
        private void DigitBtn_Click(object sender, RoutedEventArgs e)
        {
            string s = ((Button)sender).Content.ToString();

            //char[] ids = ((Button)sender).ID.ToCharArray();
            char[] ids = s.ToCharArray(); //Again, strings are indexed so why change it to an array first?
            ProcessKey(ids[0]);
        }

        private void OperBtn_Click(object sender, RoutedEventArgs e)
        {
            ProcessOperation(((Button)sender).Name.ToString());
        }
        #endregion

        #region process operation
        //requirements for ProcessOperation()
        private enum Operation
        {
            None,
            Devide,
            Multiply,
            Subtract,
            Add,
            Percent,
            Sqrt,
            OneX,
            Negate
        }
        private Operation LastOper;

        private string _last_val;
        private string LastValue
        {
            get
            {
                if (_last_val == string.Empty)
                    return "0";
                return _last_val;

            }
            set
            {
                _last_val = value;
            }
        }

        private void CalcResults()
        {
            double d;
            if (LastOper == Operation.None)
                return;

            d = Calc(LastOper);
            Display = d.ToString();

            UpdateDisplay();
        }

        private double Calc(Operation LastOper)
        {
            double d = 0.0;


            try {
            switch (LastOper)
            {
                case Operation.Devide:
                    Paper.AddArguments(LastValue + " / " + Display);
                    d = (Convert.ToDouble(LastValue) / Convert.ToDouble(Display));
                    CheckResult(d);
                    Paper.AddResult(d.ToString());
                    break;
                case Operation.Add:
                    Paper.AddArguments(LastValue + " + " + Display);
                    d = Convert.ToDouble(LastValue) + Convert.ToDouble(Display);
                    CheckResult(d);
                    Paper.AddResult(d.ToString());
                    break;
                case Operation.Multiply:
                    Paper.AddArguments(LastValue + " * " + Display);
                    d = Convert.ToDouble(LastValue) * Convert.ToDouble(Display);
                    CheckResult(d);
                    Paper.AddResult(d.ToString());
                    break;
                case Operation.Percent:
                    //Note: this is different (but make more sense) then Windows calculator
                    Paper.AddArguments(LastValue + " % " + Display);
                    d = (Convert.ToDouble(LastValue) * Convert.ToDouble(Display)) / 100.0F;
                    CheckResult(d);
                    Paper.AddResult(d.ToString());
                    break;
                case Operation.Subtract:
                    Paper.AddArguments(LastValue + " - " + Display);
                    d = Convert.ToDouble(LastValue) - Convert.ToDouble(Display);
                    CheckResult(d);
                    Paper.AddResult(d.ToString());
                    break;
                case Operation.Sqrt:
                    Paper.AddArguments("Sqrt( " + LastValue + " )");
                    d = Math.Sqrt(Convert.ToDouble(LastValue));
                    CheckResult(d);
                    Paper.AddResult(d.ToString());
                    break;
                case Operation.OneX:
                    Paper.AddArguments("1 / " + LastValue);
                    d = 1.0F / Convert.ToDouble(LastValue);
                    CheckResult(d);
                    Paper.AddResult(d.ToString());
                    break;
                case Operation.Negate:
                    d = Convert.ToDouble(LastValue) * (-1.0F);
                    break;
                }
            }

            catch
            {
                d = 0;
                Window parent = (Window)MyPanel.Parent;
                Paper.AddResult("Error");
                MessageBox.Show(parent, "Operation cannot be perfomed", parent.Title);
            }

            return d;
        }

        private void ProcessOperation(string s)
        {
            Double d = 0.0;
            switch (s)
            {
                case "BPM":
                    LastOper = Operation.Negate;
                    LastValue = Display;
                    CalcResults();
                    LastValue = Display;
                    EraseDisplay = true;
                    LastOper = Operation.None;
                    break;
                case "BDevide":
                    if (EraseDisplay)    //stil wait for a digit...
                    {  //stil wait for a digit...
                        LastOper = Operation.Devide;
                        break;
                    }
                    CalcResults();
                    LastOper = Operation.Devide;
                    LastValue = Display;
                    EraseDisplay = true;
                    break;
                case "BMultiply":
                    if (EraseDisplay)    //stil wait for a digit...
                    {  //stil wait for a digit...
                        LastOper = Operation.Multiply;
                        break;
                    }
                    CalcResults();
                    LastOper = Operation.Multiply;
                    LastValue = Display;
                    EraseDisplay = true;
                    break;
                case "BMinus":
                    if (EraseDisplay)    //stil wait for a digit...
                    {  //stil wait for a digit...
                        LastOper = Operation.Subtract;
                        break;
                    }
                    CalcResults();
                    LastOper = Operation.Subtract;
                    LastValue = Display;
                    EraseDisplay = true;
                    break;
                case "BPlus":
                    if (EraseDisplay)
                    {  //stil wait for a digit...
                        LastOper = Operation.Add;
                        break;
                    }
                    CalcResults();
                    LastOper = Operation.Add;
                    LastValue = Display;
                    EraseDisplay = true;
                    break;
                case "BEqual":
                    if (EraseDisplay)    //stil wait for a digit...
                        break;
                    CalcResults();
                    EraseDisplay = true;
                    LastOper = Operation.None;
                    LastValue = Display;
                    //val = Display;
                    break;
                case "BSqrt":
                    LastOper = Operation.Sqrt;
                    LastValue = Display;
                    CalcResults();
                    LastValue = Display;
                    EraseDisplay = true;
                    LastOper = Operation.None;
                    break;
                case "BPercent":
                    if (EraseDisplay)    //stil wait for a digit...
                    {  //stil wait for a digit...
                        LastOper = Operation.Percent;
                        break;
                    }
                    CalcResults();
                    LastOper = Operation.Percent;
                    LastValue = Display;
                    EraseDisplay = true;
                    //LastOper = Operation.None;
                    break;
                case "BOneOver":
                    LastOper = Operation.OneX;
                    LastValue = Display;
                    CalcResults();
                    LastValue = Display;
                    EraseDisplay = true;
                    LastOper = Operation.None;
                    break;
                case "BC":  //clear All
                    LastOper = Operation.None;
                    Display = LastValue = string.Empty;
                    Paper.Clear();
                    UpdateDisplay();
                    break;
                case "BCE":  //clear entry
                    LastOper = Operation.None;
                    Display = LastValue;
                    UpdateDisplay();
                    break;
                case "BMemClear":
                    Memory = 0.0F;
                    DisplayMemory();
                    break;
                case "BMemSave":
                    Memory = Convert.ToDouble(Display);
                    DisplayMemory();
                    EraseDisplay = true;
                    break;
                case "BMemRecall":
                    Display = /*val =*/ Memory.ToString();
                    UpdateDisplay();
                    //if (LastOper != Operation.None)   //using MR is like entring a digit
                    EraseDisplay = false;
                    break;
                case "BMemPlus":
                    d = Memory + Convert.ToDouble(Display);
                    Memory = d;
                    DisplayMemory();
                    EraseDisplay = true;
                    break;
            }
        }
        private void CheckResult(double d)
        {
            if (Double.IsNegativeInfinity(d) || Double.IsPositiveInfinity(d) || Double.IsNaN(d))
                throw new Exception("Illegal value");
        }
        #endregion

        #region memory operation
        private class PaperTrail
        {
            string args;

            public PaperTrail()
            {
            }
            public void AddArguments(string a)
            {
                args = a;
            }
            public void AddResult(string r)
            {
                PaperBox.Text += args + " = " + r + "\n";
            }
            public void Clear()
            {
                PaperBox.Text = string.Empty;
                args = string.Empty;
            }
        }

        private string _mem_val;
        //Get/Set Memory cell value
        private Double Memory
        {
            get
            {
                if (_mem_val == string.Empty)
                    return 0.0;
                else
                    return Convert.ToDouble(_mem_val);
            }
            set
            {
                _mem_val = value.ToString();
            }
        }

        private void DisplayMemory()
        {
            if (_mem_val != String.Empty)
                BMemBox.Text = "Memory: " + _mem_val;
            else
                BMemBox.Text = "Memory: [empty]";
        }
        #endregion
        #endregion

        #region menu callbacks
        void OnMenuAbout(object sender, RoutedEventArgs e)
        {
            Window parent = (Window)MyPanel.Parent;
            MessageBox.Show(parent, parent.Title + " - By Jossef Goldberg \nXAML to C# - By Justin Mancinelli", parent.Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        void OnMenuExit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void OnMenuStandard(object sender, RoutedEventArgs e)
        {
            //((MenuItem)ScientificMenu).IsChecked = false;
            ((MenuItem)StandardMenu).IsChecked = true; //for now always Standard
        }
        //Not implemenetd 
        void OnMenuScientific(object sender, RoutedEventArgs e)
        {
            //((MenuItem)StandardMenu).IsChecked = false; 
        }
        #endregion
    }
}