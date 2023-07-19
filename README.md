# Unity AI Texture Generator

Welcome to the Unity AI Texture Generator repository! This project aims to explore the integration of AI image generation using Stable Diffusion into Unity games. With this tool, you can experiment with using AI-generated textures for objects in your Unity projects.

## Table of Contents

- [Unity AI Texture Generator](#unity-ai-texture-generator)
  - [Table of Contents](#table-of-contents)
  - [Introduction](#introduction)
  - [Features](#features)
  - [Usage](#usage)
  - [Dependencies](#dependencies)
  - [License](#license)

## Introduction

The Unity AI Texture Generator is an experimental project that leverages the power of AI image generation using the Stable Diffusion algorithm. By incorporating AI-generated textures, developers can enhance the visual aesthetics of their Unity projects with unique and realistic textures for in-game objects.

This project serves as a developer tool that can be imported into your own Unity projects. It provides a seamless integration with Unity's workflow, allowing you to easily apply AI-generated textures to your objects and explore the creative possibilities offered by the Stable Diffusion algorithm.

## Features

- AI-generated textures using Stable Diffusion: Utilize the power of AI image generation to create realistic and unique textures for your in-game objects.
- Seamless integration with Unity: The tool seamlessly integrates into Unity's workflow, making it easy to apply AI-generated textures to your objects.
- Experimentation and customization: Explore different settings and variations of AI-generated textures to achieve the desired visual effects in your project.

## Usage

To use the Unity AI Texture Generator in your project, follow these steps:

1. Import the Unity AI Texture Generator package into your Unity project. You will need all scripts under `Assets/Editor/`, as well as `GlobalUse`, and `ImageAI` (these come from JPhilipp AIConnectors), and, under `Assets/Scripts`, you will need `InpaintingManager.cs`, `TextureManager.cs` and `TextureVersioningManager.cs`.
2. In your scene, create an empty object and call it `TextureManager`. To this, add those last three scripts.
3. Adjust the parameters and settings to customize the AI-generated texture and select the target object.
4. Fine-tune and experiment with different variations of the AI-generated texture to achieve the desired visual effect.

## Dependencies

This project relies on [AIConnectors](https://github.com/JPhilipp/AIConnectors), a project that offers a series of connectors with the Automatic1111 API.

## License

The Unity AI Texture Generator is open-source software licensed under the [MIT License](/LICENSE). You are free to use, modify, and distribute this tool for personal and commercial purposes. However, the project comes with no warranties or support. Please review the license file for more information.

**Disclaimer:** The AI image generation technology used in this project is subject to its own licensing and terms. Make sure to comply with the respective licenses and usage restrictions when using the generated textures in your projects.

---

We hope you find the Unity AI Texture Generator tool useful in your Unity projects. If you have any questions, suggestions, or encounter any issues, please don't hesitate to [open an issue](https://github.com/jncabdom/Stable-Diffusion-Unity/issues). Happy texturizing!
