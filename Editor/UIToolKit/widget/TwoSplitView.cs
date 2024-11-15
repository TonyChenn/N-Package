using UnityEngine.UIElements;

public class TwoSplitView : TwoPaneSplitView
{
	public new class UxmlFactory : UxmlFactory<TwoPaneSplitView, UxmlTraits> { }

	public TwoSplitView() : base() { }

	/// <summary>
	/// Parameterized constructor.
	/// </summary>
	/// <param name="fixedPaneIndex">0 for setting first child as the fixed pane, 1 for the second child element.</param>
	/// <param name="fixedPaneStartDimension">Set an inital width or height for the fixed pane.</param>
	/// <param name="orientation">Orientation of the split view.</param>
	public TwoSplitView(int fixedPaneIndex, float fixedPaneStartDimension, TwoPaneSplitViewOrientation orientation) 
		: base(fixedPaneIndex, fixedPaneStartDimension, orientation)
	{
	}
}
