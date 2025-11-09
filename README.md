 # <img width="32" height="32" alt="GasIcon" src="https://github.com/user-attachments/assets/9f14ac9e-a9e6-4128-9738-d331428a2a4b" />  Gas Simulation based on Cellular Automata

This is a gas simulation I developed for another project, with an added GUI for user interactions and testing. The goal is to make a sim thats accurate *enough*, while being fast, and featuring pressure, temperature, wind, and multiple gases.

This project demonstrates my implementation of CA for gas simulation in Godot with C#, MVVM applied as "Logic Node <-> State <-> Control Node (UI)" pattern, and some Property manipulation and Inspector-like UI generation.

## Why this is awesome (Features):
### Software
- **Decoupled Architectrue (MVVM Pattern):** Logic, UI, and data State are all separated
- **Observer pattern:** Links UI event to the States
- **Dynamic UI:** Adding new variables and properties generates new entries in the GUI, with the correct type

### Physics
- **Cell-based volume:** Each cell represents 1 unit of volume, allowing to model both small and large changes in gas, while remaining perfomant
- **Ideal gas model**
- **Diffusion based on discrete Fick's Law**
- **Free (Joules) Expansion of gas**
- **Advection:** Coming soon(tm)
- **Multiple false-color filters** to visualise changes in temperature, pressure, wind, etc.
- **Real time interactions:** Spawn new gas at any temperature, change layout of walls, or spawn a physics body thats affected by wind

## Videos

### Sim Preview:
https://github.com/user-attachments/assets/d7678646-3539-4784-87b5-f23b45f50977

### Dynamic property generation in the GUI:
https://github.com/user-attachments/assets/c1562176-1ed2-4b17-9691-80047391014f

### MVVM Diagram of the project:
<img width="753" height="462" alt="MVVMSchema" src="https://github.com/user-attachments/assets/4b626151-7c44-44cb-b0f1-71b7aad49158" />
