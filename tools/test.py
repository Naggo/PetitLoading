from pathlib import Path
import os
import subprocess
import tkinter
import tkinter.filedialog as filedialog


flagpath = Path(__file__).parent / "flag.tmp"
invokerpath = Path(__file__).parents[1] / "src/PetitLoading/PyScripts~/invoker.py"
rectstring = "32,128,128,128,SE"


imagespath: tkinter.StringVar


def is_windows():
    return os.name == "nt"

def is_mac():
    return os.name == "posix"


def setpath():
    path = filedialog.askdirectory()
    imagespath.set(path)


def start():
    flagpath.touch()
    name = "python3"
    if is_windows():
        name = "pythonw"
    cmd = [name, str(invokerpath), str(flagpath), imagespath.get(), rectstring]
    subprocess.Popen(cmd)


def stop():
    flagpath.unlink(missing_ok=True)


def main():
    global imagespath

    root = tkinter.Tk()
    root.geometry("300x60")

    button = tkinter.Button(root, text="Start", width=15, command=start)
    button.grid(column=0, row=0)
    button = tkinter.Button(root, text="Stop", width=15, command=stop)
    button.grid(column=1, row=0)
    button = tkinter.Button(root, text="Path", width=8, command=setpath)
    button.grid(column=2, row=0)

    imagespath = tkinter.StringVar(root)
    label = tkinter.Label(root, textvariable=imagespath, wraplength=290, justify="left")
    label.grid(column=0, columnspan=3, row=1, sticky="w")

    root.mainloop()


if __name__ == "__main__":
    main()
