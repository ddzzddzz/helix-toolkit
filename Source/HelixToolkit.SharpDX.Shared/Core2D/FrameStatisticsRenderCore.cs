﻿/*
The MIT License (MIT)
Copyright (c) 2018 Helix Toolkit contributors
*/
using D2D = SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX;
using System;

#if NETFX_CORE
namespace HelixToolkit.UWP.Core2D
#else
namespace HelixToolkit.Wpf.SharpDX.Core2D
#endif
{
    using Utilities;
    public class FrameStatisticsRenderCore : RenderCore2DBase
    {
        private IRenderStatistics statistics;

        private D2D.Brush foreground = null;
        public D2D.Brush Foreground
        {
            set
            {
                var old = foreground;
                if (SetAffectsRender(ref foreground, value))
                {
                    RemoveAndDispose(ref old);
                    Collect(value);
                }
            }
            get
            {
                return foreground;
            }
        }

        private D2D.Brush background = null;
        public D2D.Brush Background
        {
            set
            {
                var old = background;
                if (SetAffectsRender(ref background, value))
                {
                    RemoveAndDispose(ref old);
                    Collect(value);
                }
            }
            get
            {
                return background;
            }
        }

        private TextLayout textLayout;
        private Factory factory;
        private TextFormat format;
        private RectangleF renderBound = new RectangleF(0, 0, 100, 0);
        private string previousStr = "";

        protected override bool OnAttach(IRenderHost target)
        {
            factory = Collect(new Factory(FactoryType.Isolated));
            format = Collect(new TextFormat(factory, "Arial", 12));
            previousStr = "";
            this.statistics = target.RenderStatistics;
            return base.OnAttach(target);
        }

        protected override bool CanRender(IRenderContext2D context)
        {
            return base.CanRender(context) && statistics != null && statistics.FrameDetail != RenderDetail.None;
        }

        protected override void OnRender(IRenderContext2D context)
        {
            var str = statistics.GetDetailString();
            if (str != previousStr)
            {
                previousStr = str;
                RemoveAndDispose(ref textLayout);
                textLayout = Collect(new TextLayout(factory, str, format, float.MaxValue, float.MaxValue));
            }
            var metrices = textLayout.Metrics;
            renderBound.Width = Math.Max(metrices.Width, renderBound.Width);
            renderBound.Height = metrices.Height;
            context.DeviceContext.Transform = Matrix3x2.Translation((float)context.ActualWidth - renderBound.Width, 0);                                     
            context.DeviceContext.FillRectangle(renderBound, background);
            context.DeviceContext.DrawTextLayout(Vector2.Zero, textLayout, foreground);
        }
    }
}