using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace mgen_textBlockSelection
{
    internal sealed class SelectableTextBlock : TextBlock
    {
        #region Fields and Properties

        private readonly MenuItem _copyMenu;
        private TextPointer _endpoz;
        private TextPointer _startpoz;

        private bool HasSelection => Selection != null && !Selection.IsEmpty;

        private TextRange Selection { get; set; }

        #endregion

        #region  Constructors

        public SelectableTextBlock()
        {
            Focusable = true;
            var contextMenu = new ContextMenu();
            ContextMenu = contextMenu;

            _copyMenu = new MenuItem
            {
                Header = "复制",
                InputGestureText = "Ctrl + C"
            };
            _copyMenu.Click += (ss, ee) => { Copy(); };
            contextMenu.Items.Add(_copyMenu);

            var selectAllMenu = new MenuItem
            {
                Header = "全选",
                InputGestureText = "Ctrl + A"
            };
            selectAllMenu.Click += (ss, ee) => { SelectAll(); };
            contextMenu.Items.Add(selectAllMenu);

            ContextMenuOpening += contextMenu_ContextMenuOpening;
        }

        #endregion

        public event EventHandler SelectionChanged;

        #region  Methods

        private void ClearSelection()
        {
            var contentRange = new TextRange(ContentStart, ContentEnd);
            contentRange.ApplyPropertyValue(TextElement.BackgroundProperty,"#00000000");
            Selection = null;
        }

        public bool Copy()
        {
            if (HasSelection)
            {
                Clipboard.SetDataObject(Selection.Text);
                return true;
            }
            return false;
        }

        private void SelectAll()
        {
            Selection = new TextRange(ContentStart, ContentEnd);
            Selection.ApplyPropertyValue(TextElement.BackgroundProperty, SelectionBrush);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
                if (e.Key == Key.C)
                    Copy();
                else if (e.Key == Key.A)
                    SelectAll();

            base.OnKeyUp(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var point = e.GetPosition(this);
            _startpoz = GetPositionFromPoint(point, true);
            CaptureMouse();
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
            ReleaseMouseCapture();
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var point = e.GetPosition(this);
                _endpoz = GetPositionFromPoint(point, true);

                ClearSelection();
                Selection = new TextRange(_startpoz, _endpoz);
                Selection.ApplyPropertyValue(TextElement.BackgroundProperty, SelectionBrush);
                CommandManager.InvalidateRequerySuggested();

                OnSelectionChanged(EventArgs.Empty);
            }

            base.OnMouseMove(e);
        }

        private void OnSelectionChanged(EventArgs e)
        {
            var handler = SelectionChanged;
            handler?.Invoke(this, e);
        }

        private void contextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            _copyMenu.IsEnabled = HasSelection;
        }

        #endregion

        #region SelectionBrush

        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register("SelectionBrush", typeof(Brush), typeof(SelectableTextBlock),
                new FrameworkPropertyMetadata(Brushes.Orange));

        public Brush SelectionBrush
        {
            get => (Brush) GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }

        #endregion
    }
}