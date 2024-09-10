from dataclasses import dataclass
from typing import TypeAlias, Literal


Anchor: TypeAlias = Literal["NE", "NW", "SE", "SW"]


@dataclass
class AnchoredRect:
    x: int
    y: int
    width: int
    height: int
    anchor: Anchor

    def getgeometry(self, screen_width: int, screen_height: int) -> str:
        x = self._calculate_x(screen_width)
        y = self._calculate_y(screen_height)
        return f"{self.width}x{self.height}+{x}+{y}"

    def _calculate_x(self, screen_width: int) -> int:
        if self.anchor[1] == "W":
            return self.x
        else:
            return screen_width - (self.x + self.width)

    def _calculate_y(self, screen_height: int) -> int:
        if self.anchor[0] == "N":
            return self.y
        else:
            return screen_height - (self.y + self.height)
