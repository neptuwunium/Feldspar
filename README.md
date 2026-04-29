<!--
SPDX-FileCopyrightText: 2026 Neptuwunium

SPDX-License-Identifier: EUPL-1.2
-->

# Feldspar

Note: the project is only a library, no CLI or GUI exists yet.

A resource manager for KTGL2 (also known as "Katana" Engine and about five other names[^1]).

[^1]: Alchemy, Katana, Motor, KTGL Sample, Soft Engine.

Research for the file formats is in https://github.com/neptuwunium/bt/

## ResourceDatabase

Depending on how complex it will be to retroport RESPACK / Elixir to the newer Resource system,
this project may at some point be split up into two parts. Before and after RDB.

The resource database system allows for much, much easier asset loading.

However, my ultimate goal is still to have a map viewer for Dissidia NT and Atelier Ryza/Yumia.
