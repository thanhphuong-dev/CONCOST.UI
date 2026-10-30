"""Renumber the library to 1.0.0 — the first public release."""
import io

def edit(path, pairs):
    s = io.open(path, encoding='utf-8', newline='').read()
    for old, new in pairs:
        assert old in s, (path, old[:60])
        s = s.replace(old, new)
    io.open(path, 'w', encoding='utf-8', newline='').write(s)
    print('updated', path)

edit('Directory.Build.props', [
    ('<Version>3.7.0</Version>', '<Version>1.0.0</Version>'),
    ('<PackageVersion>3.7.0</PackageVersion>', '<PackageVersion>1.0.0</PackageVersion>'),
    ('<AssemblyVersion>3.7.0.0</AssemblyVersion>', '<AssemblyVersion>1.0.0.0</AssemblyVersion>'),
    ('<FileVersion>3.7.0.0</FileVersion>', '<FileVersion>1.0.0.0</FileVersion>'),
])

edit('README.md', [
    ('Download `CONCOST.UI.3.7.0.nupkg`', 'Download `CONCOST.UI.1.0.0.nupkg`'),
    ('dotnet add package CONCOST.UI --version 3.7.0', 'dotnet add package CONCOST.UI --version 1.0.0'),
])

edit('README.vi.md', [
    ('Tải `CONCOST.UI.3.7.0.nupkg`', 'Tải `CONCOST.UI.1.0.0.nupkg`'),
    ('dotnet add package CONCOST.UI --version 3.7.0', 'dotnet add package CONCOST.UI --version 1.0.0'),
])

edit('docs/index.html', [
    ('<span class="badge-ver" id="verBadge">v3.7.0</span>',
     '<span class="badge-ver" id="verBadge">v1.0.0</span>'),
])

edit('docs/app.js', [('"v3.7.0"', '"v1.0.0"')])

edit('samples/CONCOST.UI.Showcase/MainWindow.xaml', [
    ('<TextBlock Text="v3.0.0 Pro"', '<TextBlock Text="v1.0.0"'),
])
