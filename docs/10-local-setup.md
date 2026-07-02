# Local Setup

How to run SilverHub locally. The architecture docs (`01`–`09`) cover the design
and the reasoning behind these choices; this file is just the steps to get it
running.

## Prerequisites

| Tool           | Version         | Install                                                    |
| -------------- | --------------- | ---------------------------------------------------------- |
| .NET SDK       | 10 (LTS)        | <https://dotnet.microsoft.com/download/dotnet/10.0>        |
| Docker Desktop | latest          | <https://www.docker.com/products/docker-desktop/>          |
| Node.js        | 24 (Active LTS) | <https://nodejs.org> — or `nvm`/`fnm`, which read `.nvmrc` |

The .NET SDK version is pinned in `global.json` and the Node version in `.nvmrc`,
so version managers select the right toolchain automatically.

Verify:

```bash
dotnet --version   # 10.0.x
docker --version
node --version     # v24.x
```

## Getting started

```bash
git clone https://github.com/Midnight-Programmer/SilverHub-Web-App.git
cd SilverHub-Web-App
```

Then follow the section for the part you want to run:

- **Local database** — _(added in Task 2)_
- **Backend API** — _(added in Task 1–2)_
- **Frontend** — _(added in Task 3)_
- **Full stack, end to end** — _(added in Task 4)_
