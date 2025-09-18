#!/usr/bin/env python3
"""
简单迁移 Unity 项目的脚本
- 在代码里直接修改 source 和 target 两个路径字符串
- 按 Unity .gitignore 的规则过滤掉不用复制的文件/文件夹
- 默认是复制（保留原始文件），如果你想改成移动，把 shutil.copy2 换成 shutil.move 即可
"""

import os
import shutil
import sys
from pathlib import Path
import pathspec

# ==== 直接在这里改路径 ====
# NOTE: 原来这里有个路径书写错误 (示例: "D:E:\\..."), 导致脚本没有遍历到任何文件。
# 依据工作区信息，默认改为当前项目所在的 E: 驱动器路径。如果你的项目在其他位置，
# 请在这里手动修改 `source` 变量为正确的绝对路径。
source = r"E:\\Unity Project\\3DMappingAI"     # Unity 项目源目录
target = r"E:\\ETH\\Papers\\Sketch2Terrain_CHI24\\Github_open_source\\Sketch2Terrain\\Unity_Project"  # 迁移目标目录
# ==========================

# 默认 Unity .gitignore（简化版，可以自行扩展）
UNITY_GITIGNORE = r"""
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
/[Mm]emoryCaptures/
/[Rr]ecordings/
.vs/
.gradle/
*.csproj
*.sln
*.user
*.pidb
*.pdb
*.mdb
*.apk
*.aab
*.unitypackage
*.app
"""

def build_pathspec():
    lines = [l for l in UNITY_GITIGNORE.splitlines() if l and not l.strip().startswith("#")]
    return pathspec.PathSpec.from_lines("gitwildmatch", lines)

def migrate(source: Path, target: Path, spec: pathspec.PathSpec):
    copied, skipped = 0, 0
    print(f"source: {source}")
    for root, dirs, files in os.walk(source):
        rel_dir = os.path.relpath(root, source)
        if rel_dir == ".":
            rel_dir = ""
        rel_dir_posix = rel_dir.replace("\\", "/").lstrip("/")
        
        # 目录忽略
        if rel_dir_posix and spec.match_file(rel_dir_posix):
            dirs[:] = []  # 不再深入
            continue

        for f in files:
            rel_path = (rel_dir_posix + "/" + f).lstrip("/") if rel_dir_posix else f
            if spec.match_file(rel_path):
                print(f"跳过文件: {rel_path}")
                skipped += 1
                continue

            src_file = Path(root) / f
            dst_file = target / rel_path
            dst_file.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(src_file, dst_file)
            copied += 1

    print(f"完成迁移: 复制 {copied} 个文件，跳过 {skipped} 个文件.")

if __name__ == "__main__":
    src = Path(source)
    dst = Path(target)
    dst.mkdir(parents=True, exist_ok=True)

    spec = build_pathspec()
    migrate(src, dst, spec)
