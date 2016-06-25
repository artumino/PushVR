using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;

using SteamVR_HUDCenter.Elements;
using SteamVR_HUDCenter.Elements.WPF;

using Valve.VR;
using OpenTK.Graphics.OpenGL;
using System.Windows.Media.Imaging;
using SteamVR_HUDCenter;
using System.Windows.Threading;

namespace PushbulletVR.Overlays
{
    public class WPFOverlay : Overlay
    {
        public IVRUserControl ControlToRender { get; private set; }
        public Dispatcher Dispatcher { get; private set; }

        private int? TextureID;
        private bool DynSize = false;
        private Texture_t WPFTexture;
        private int CalmpHeight = 850;
        private float CurrentHeight = 0.0f;
        private Bitmap CurrentBitmap;

        public WPFOverlay(string FriendlyName,
            string ThumbnailPath,
            IVRUserControl ControlToRender,
            Dispatcher Dispatcher,
            float? Width = null)
            : base(FriendlyName, ThumbnailPath, 1.0f, VROverlayInputMethod.Mouse)
        {
            this.ControlToRender = ControlToRender;
            this.ControlToRender.OnRendered += ControlToRender_OnRendered;
            this.Dispatcher = Dispatcher;

            if (Width.HasValue)
                SetOverlaySize(Width.Value);
            else
                DynSize = true;
        }

        public override void Init(HUDCenterController Controller)
        {
            base.Init(Controller);

            OpenVR.Overlay.SetOverlayFlag(this.Handle, VROverlayFlags.ShowTouchPadScrollWheel, true);
            OpenVR.Overlay.SetOverlayFlag(this.Handle, VROverlayFlags.SendVRScrollEvents, true);
        }

        private void ControlToRender_OnRendered(System.Windows.Media.Imaging.RenderTargetBitmap bitmapRend)
        {
            CurrentHeight = 0;
            if (Controller != null && bitmapRend == null)
            {
                OpenVR.Overlay.ClearOverlayTexture(this.Handle);
                if (this.CurrentBitmap != null)
                {
                    this.CurrentBitmap.Dispose();
                    this.CurrentBitmap = null;
                }
                return;
            }
            
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapRend));
            encoder.Save(stream);

            CurrentBitmap = new System.Drawing.Bitmap(stream);
            RenderBitmap();
        }

        public void RenderBitmap()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                int? OldID = TextureID;

                TextureID = GL.GenTexture();

                if (OldID.HasValue)
                    GL.DeleteTexture(OldID.Value);

                if (CurrentBitmap != null)
                {
                    using (System.Drawing.Bitmap iconOriginal = new System.Drawing.Bitmap(CurrentBitmap.Width, Math.Min(Math.Abs(CurrentBitmap.Height - (int)CurrentHeight), CalmpHeight), CurrentBitmap.PixelFormat))
                    using (Graphics g = Graphics.FromImage(iconOriginal))
                    {
                        if (DynSize)
                            this.SetOverlaySize((iconOriginal.Width / 15.0f) * 0.0254f); //PPI to Meters

                        g.DrawImage(CurrentBitmap, new Rectangle(0, 0, iconOriginal.Width, iconOriginal.Height), new Rectangle(0, (int)CurrentHeight, iconOriginal.Width, iconOriginal.Height), GraphicsUnit.Pixel);

                        iconOriginal.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        System.Drawing.Imaging.BitmapData TextureData =
                        iconOriginal.LockBits(
                            new System.Drawing.Rectangle(0, 0, iconOriginal.Width, iconOriginal.Height),
                            System.Drawing.Imaging.ImageLockMode.ReadOnly,
                            System.Drawing.Imaging.PixelFormat.Format32bppPArgb
                        );

                        GL.BindTexture(TextureTarget.Texture2D, TextureID.Value);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, TextureData.Scan0);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

                        iconOriginal.UnlockBits(TextureData);

                        WPFTexture = new Texture_t();
                        WPFTexture.eType = EGraphicsAPIConvention.API_OpenGL;
                        WPFTexture.eColorSpace = EColorSpace.Auto;
                        WPFTexture.handle = (IntPtr)TextureID.Value;

                        if (Controller != null)
                            OpenVR.Overlay.SetOverlayTexture(this.Handle, ref WPFTexture);
                    }
                }
            }));
        }

        public override void OnVREvent_Scroll(VREvent_Data_t Data)
        {
            base.OnVREvent_Scroll(Data);
            
            if (CurrentBitmap != null)
            {
                float Delta = -Data.scroll.ydelta * (100 + (100 * (Data.scroll.repeatCount / 5)));
                if (Delta > 0)
                {
                    float maxScroll = CurrentBitmap.Height - CalmpHeight - CurrentHeight ;
                    this.CurrentHeight += maxScroll > 0 ? Math.Min(maxScroll, Delta) : 0.0f;
                }
                else
                    this.CurrentHeight = (this.CurrentHeight + Delta) >= 0 ? (this.CurrentHeight + Delta) : 0.0f;
                RenderBitmap();
            }
        }
    }
}
