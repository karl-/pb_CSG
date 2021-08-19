# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.0]

### Bug Fixes

- Fixed potential crash or stack overflow if `Plane.SplitPolygon` fails to correctly identify co-planar triangles.

### Changes

- Remove `pb_` prefix from type names.
- Rename `Boolean` class to `CSG`.