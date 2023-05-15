using System;
using Markdig;
using UnityEditor;
using UnityEngine;
using Markdig.Extensions.JiraLinks;
using Markdig.Extensions.Tables;


public class MarkdownViewer
{
    public static readonly Vector2 Margin = new Vector2(6.0f, 4.0f);

    private GUISkin mSkin = null;
    private string mText = string.Empty;
    private string mCurrentPath = string.Empty;
    private HandlerImages mHandlerImages = new HandlerImages();
    private HandlerNavigate mHandlerNavigate = new HandlerNavigate();

    private Func<float> mViewWidthProvider = () => EditorGUIUtility.currentViewWidth;

    private Layout mLayout = null;
    private bool mRaw = false;

    private static History mHistory = new History();

    public MarkdownViewer(string path, string content)
    {
        string skinName = Path_Markdown.UseDarkMarkdownTheme ? "Skin/MarkdownSkinQS" : "Skin/MarkdownViewerSkin";
        mSkin = Resources.Load<GUISkin>(skinName);
        mCurrentPath = path;
        mText = content;

        mHistory.OnOpen(mCurrentPath);
        mLayout = ParseDocument();

        mHandlerImages.CurrentPath = mCurrentPath;

        mHandlerNavigate.CurrentPath = mCurrentPath;
        mHandlerNavigate.History = mHistory;
        mHandlerNavigate.FindBlock = (id) => mLayout.Find(id);
        mHandlerNavigate.ScrollTo = (pos) => { }; // TODO: mScrollPos.y = pos;
    }

    public MarkdownViewer(string path, string content, Func<float> viewWidthProvider) : this(path, content)
    {
        if (viewWidthProvider == null)
        {
            throw new ArgumentNullException(nameof(viewWidthProvider));
        }

        mViewWidthProvider = viewWidthProvider;
    }


    //------------------------------------------------------------------------------

    public bool Update()
    {
        return mHandlerImages.Update();
    }


    //------------------------------------------------------------------------------

    Layout ParseDocument()
    {
        var context = new Context(mSkin, mHandlerImages, mHandlerNavigate);
        var builder = new LayoutBuilder(context);
        var renderer = new RendererMarkdown(builder);

        var pipelineBuilder = new MarkdownPipelineBuilder()
            .UseAutoLinks()
        ;

        if (!string.IsNullOrEmpty(Preferences.JIRA))
        {
            pipelineBuilder.UseJiraLinks(new JiraLinkOptions(Preferences.JIRA));
        }


        if (Preferences.PipedTables)
        {
            pipelineBuilder.UsePipeTables(new PipeTableOptions
            {
                RequireHeaderSeparator = Preferences.PipedTablesRequireRequireHeaderSeparator
            });
        }


        var pipeline = pipelineBuilder.Build();
        pipeline.Setup(renderer);

        var doc = Markdown.Parse(mText, pipeline);
        renderer.Render(doc);

        return builder.GetLayout();
    }


    //------------------------------------------------------------------------------

    private void ClearBackground(float height)
    {
        var rectFullScreen = new Rect(0.0f, 0.0f, Screen.width, Mathf.Max(height, Screen.height));
        GUI.DrawTexture(rectFullScreen, mSkin.window.normal.background, ScaleMode.StretchToFill, false);
    }


    //------------------------------------------------------------------------------

    public void Draw()
    {
        GUI.skin = mSkin;
        GUI.enabled = true;

        // useable width of inspector windows

        var contentWidth = mViewWidthProvider() - mSkin.verticalScrollbar.fixedWidth - 2.0f * Margin.x;


        // draw content

        if (mRaw)
        {
            var style = mSkin.GetStyle("raw");
            var width = contentWidth - mSkin.button.fixedHeight;
            var height = style.CalcHeight(new GUIContent(mText), width);

            ClearBackground(height);
            EditorGUILayout.SelectableLabel(mText, style, new GUILayoutOption[] { GUILayout.Width(width), GUILayout.Height(height) });
        }
        else
        {
            ClearBackground(mLayout.Height);
            DrawMarkdown(contentWidth);
        }

        DrawToolbar(contentWidth);
    }

    //------------------------------------------------------------------------------

    void DrawToolbar(float contentWidth)
    {
        var style = GUI.skin.button;
        var size = style.fixedHeight;
        var btn = new Rect(Margin.x + contentWidth - size, Margin.y, size, size);

        if (GUI.Button(btn, string.Empty, GUI.skin.GetStyle(mRaw ? "btnRaw" : "btnFile")))
        {
            mRaw = !mRaw;
        }

        if (mRaw == false)
        {
            if (mHistory.CanForward)
            {
                btn.x -= size;

                if (GUI.Button(btn, string.Empty, GUI.skin.GetStyle("btnForward")))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(mHistory.Forward());
                }
            }

            if (mHistory.CanBack)
            {
                btn.x -= size;

                if (GUI.Button(btn, string.Empty, GUI.skin.GetStyle("btnBack")))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(mHistory.Back());
                }
            }
        }
    }

    //------------------------------------------------------------------------------

    void DrawMarkdown(float width)
    {
        switch (Event.current.type)
        {
            case EventType.Ignore:
                break;

            case EventType.ContextClick:
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("View Source"), false, () => mRaw = !mRaw);
                menu.ShowAsContext();

                break;

            case EventType.Layout:
                GUILayout.Space(mLayout.Height);
                mLayout.Arrange(width);
                break;

            default:
                mLayout.Draw();
                break;
        }
    }







    // add by tonychenn
    private Vector2 scrollPos;
    public void DrawWithRect(Rect rect)
    {
        GUI.skin = mSkin;
        GUI.enabled = true;


        // clear background

        var rectFullScreen = new Rect(rect.x, rect.y, rect.width, rect.height);
        GUI.DrawTexture(rectFullScreen, mSkin.window.normal.background, ScaleMode.StretchToFill, false);

        // scroll window

        var padLeft = 8.0f;
        var padRight = 4.0f;
        var padHoriz = padLeft + padRight;
        var scrollWidth = GUI.skin.verticalScrollbar.fixedWidth;
        var minWidth = rect.width - scrollWidth - padHoriz;
        var maxHeight = mRaw ? GUI.skin.GetStyle("raw").CalcHeight(new GUIContent(mText), minWidth) : mLayout.Height;

        var hasScrollbar = maxHeight >= rect.height;
        var contentWidth = hasScrollbar ? minWidth : rect.width - padHoriz;
        var rectContent = new Rect(-padLeft, 0.0f, contentWidth, maxHeight);

        // draw content

        using (var scroll = new GUI.ScrollViewScope(rect, scrollPos, rectContent))
        {
            GUILayout.BeginHorizontal();

            scrollPos = scroll.scrollPosition;

            if (mRaw)
            {
                rectContent.width = minWidth - GUI.skin.button.fixedWidth;
                EditorGUI.SelectableLabel(rectContent, mText, GUI.skin.GetStyle("raw"));
            }
            else
            {
                DrawMarkdown(rect.width);
            }

            GUILayout.Space(20);// scroll bar 增加 20 个像素
            GUILayout.EndHorizontal();
        }

        var size = GUI.skin.button.fixedHeight;
        var btn = new Rect(rect.x+rect.width-size, Margin.y, size, size);

        if (GUI.Button(btn, string.Empty, GUI.skin.GetStyle(mRaw ? "btnRaw" : "btnFile")))
        {
            mRaw = !mRaw;
        }
    }

    public void UpdateContent(string md)
    {
        if (mText != md)
        {
            mText = md;
            mLayout = ParseDocument();
        }
    }
}
