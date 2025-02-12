# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- Implement different priprities' types for systems. Separate all systems into EARLY, NORMAL and LATE executed.

- Now only a single world's context is supported. In later builds there should be multiple worlds supported.

## [0.3.0] - 2019-06-12

### Added

- A components iterator that provides easy way of enumerating over all components that some entity has

- Add custom debug inspectors for WorldContextsManager, EntityObserver and SystemManagerObserver types

- Implement ToString() method for Entity type

- Add template project for Unity3D and corresponding tutorial sample, which demonstrates how to integrate TinyECS into Unity3D

### Fixed

- Fixed an issue "Created with GameObjectFactory entities don't have TViewComponent attached to them" #11

- Fixed an issue "Reactive systems don't response on to events that are generated with IUpdateSystem implementations" #6

### Changed

- Now all reactive systems are executed after all IUpdateSystems

## [0.2.21] - 2019-05-30

### Added

- A components iterator that provides easy way of enumerating over all components that some entity has

- Add custom debug inspectors for WorldContextsManager, EntityObserver and SystemManagerObserver types

- Implement ToString() method for Entity type

### Fixed

- Now you can subscribe same reference to a system in different roles. For example, as initializing system and as update system at the same time

## [0.2.0] - 2019-05-02

### Added

- A bunch of helper types for TinyECS were added into the project to implement an integration with Unity3D.

- Support of static and dynamically created views was implemented.

- A new type of entities was introduced which are disposable entities.

### Changed

- Now event manager separates types of delivering events to their listeners. Single- and broadcasting are now supported.

### Fixed

- IEntityManager.GetEntitiesWithAll's implementation was fixed. 

- EntityManager.DestroyEntity's implementation was fixed. Now it correctly destroys given entities.