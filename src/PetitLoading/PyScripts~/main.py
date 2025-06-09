from pathlib import Path
import ctypes
import os
import random
import sys
import tkinter

from PIL import Image, ImageTk, GifImagePlugin

from anchoredrect import AnchoredRect


RECT = AnchoredRect(32, 128, 128, 128, "SE")
SUPPORTED_FILES = [
    ".gif",
    ".png"
]


def is_windows():
    return os.name == "nt"

def is_mac():
    return os.name == "posix"


class FrameTk:
    def __init__(self, image: Image.Image, interval: int):
        self.image = image
        self.interval = interval
        self.image_tk = ImageTk.PhotoImage(image)


class LoadingWindow:
    """original: https://qiita.com/magiclib/items/89447ffbf42371cd6538"""


    def __init__(self, image: Image.Image, rect: AnchoredRect):
        self.image = image
        self.rect = rect

        self.window = tkinter.Tk()
        self.window.wm_title("Petit Loading")
        self.window.wm_geometry(rect.getgeometry(
            self.window.winfo_screenwidth(),
            self.window.winfo_screenheight()
        ))

        self.current_frame = 0
        self.frames: list[FrameTk]
        self.resolve_image()

        self.canvas: tkinter.Canvas
        self.init_canvas()

        self.set_transparent()

        self.window.wm_attributes("-topmost", True)
        self.window.wm_resizable(False, False)
        if is_mac():
            self.window.update_idletasks()
        self.window.wm_overrideredirect(True)
        self.window.bind('<Visibility>', self.on_visibility_changed)

        self.pmenu = tkinter.Menu(self.window, tearoff=0)
        self.pmenu.add_command(label="Hide", command=self.hide_window)
        self.canvas.bind("<Button-3>", self.on_right_clicked)

        self.flagpath: Path

    def resolve_image(self):
        # 画像データを展開
        self.frames = []
        if (isinstance(self.image, GifImagePlugin.GifImageFile) and self.image.is_animated):
            for i in range(self.image.n_frames):
                self.image.seek(i)
                frame = self.convert_image(self.image)
                self.frames.append(FrameTk(frame, frame.info["duration"]))
        else:
            frame = self.convert_image(self.image)
            self.frames.append(FrameTk(frame, 500))

    def convert_image(self, image: Image.Image) -> Image.Image:
        # 画像を表示用に加工
        result = image.convert("RGBA")
        result.thumbnail((self.rect.width, self.rect.height), Image.NEAREST)
        return result

    def init_canvas(self):
        # canvasを作成
        self.canvas = tkinter.Canvas(
            self.window,
            width=self.rect.width,
            height=self.rect.height
        )

        # 枠を消すためにマイナス値を指定
        if is_mac():
            FRAME_OFFSET = -3
        else:
            FRAME_OFFSET = -2
        self.canvas.place(x=FRAME_OFFSET,
                          y=FRAME_OFFSET)

        # canvasに画像を表示
        frame = self.frames[0]
        self.canvas.create_image(
            self.rect.width / 2,
            self.rect.height / 2,
            image=frame.image_tk,
            tag="img"
        )

    def set_transparent(self):
        if is_windows():
            BG_COLOR = "snow"
            self.window.wm_attributes("-transparentcolor", BG_COLOR)
            self.canvas.configure(bg=BG_COLOR)

        elif is_mac():
            BG_COLOR = "systemTransparent"
            self.window.wm_attributes("-transparent", True)
            self.window.configure(bg=BG_COLOR)
            self.canvas.configure(bg=BG_COLOR)

    def start_animation(self, flagpath):
        # ループを開始
        self.flagpath = Path(flagpath)
        self.window.after(self.frames[0].interval, self.update)
        self.window.mainloop()

    def update(self):
        # フラグファイルを確認
        if (not self.flagpath.exists()):
            self.window.destroy()
            return

        next_frame = (self.current_frame + 1) % len(self.frames)
        self.set_frame(next_frame)
        self.window.after(self.frames[self.current_frame].interval, self.update)

    def set_frame(self, index: int):
        # 画像を更新
        self.current_frame = index
        self.canvas.itemconfigure("img", image=self.frames[index].image_tk)

    def hide_window(self):
        # ウィンドウを最小化
        self.window.wm_iconphoto(False, self.frames[self.current_frame].image_tk)
        self.window.wm_overrideredirect(False)
        self.window.wm_iconify()

    def on_visibility_changed(self, event: tkinter.Event):
        # ウィンドウの状態を取得
        state = self.window.wm_state()
        if state == "normal":
            self.window.wm_overrideredirect(True)

    def on_right_clicked(self, event: tkinter.Event):
        # ポップアップを表示
        self.pmenu.post(event.x_root, event.y_root)


def get_image_default():
    path = Path(__file__)
    return Image.open(path.parent / "Placeholder.gif")


def get_image(folderpath):
    imagefolder = Path(folderpath)
    if (not imagefolder.is_dir()):
        return get_image_default()

    imagepaths = []
    for path in imagefolder.iterdir():
        if path.suffix in SUPPORTED_FILES:
            imagepaths.append(path)

    if (len(imagepaths) > 0):
        imagepath = random.choice(imagepaths)
        return Image.open(imagepath)
    else:
        return get_image_default()


def get_rect(rectstring: str):
    parts = rectstring.split(",")
    try:
        rect = AnchoredRect(
            int(parts[0]),
            int(parts[1]),
            int(parts[2]),
            int(parts[3]),
            parts[4]
        )
        return rect
    except ValueError:
        return RECT


def main():
    if is_windows():
        ctypes.windll.shcore.SetProcessDpiAwareness(True)

    image = get_image(sys.argv[2])
    rect = get_rect(sys.argv[3])

    root = LoadingWindow(image, rect)
    root.start_animation(sys.argv[1])


if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print(e)
        input()
