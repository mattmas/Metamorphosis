# Metamorphosis
A tool for watching changes between Revit models, addin and Dynamo.

When you receive a new Revit model from a partner - it's a challenge to know exactly what has changed.
The Metamorphosis tool attempts to assist you; by enabling you to take a snapshot of an older model, and compare it against the newer model.
Metamorphosis captures all Revit parameter data (instance and type) as well as basic geometric data. While this is not a guarantee that all changes will be detected, we believe it will help significantly.

It has two components:
  - A Revit Addin with two commands:
     - Take a snapshot of the current model, to a file.
     - Compare the current model to a previous snapshot file.
  - A Dynamo script to take the results of the comparison and use Dynamo Mandrill to show a dashboard of the changes.


[![Metamorphosis Video]https://youtu.be/uUcwH8GvlDY/0.jpg)](https://youtu.be/uUcwH8GvlDY)

This tool was developed at the CORE Studio AEC Symposium and Hackathon, December 2016 (a 26-hour project).
