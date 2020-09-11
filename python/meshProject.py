import python.wavefront as wav

if __name__ == "__main__":
    r = 2 # radius of S^3 when projecting
    input = wav.load_obj("Sphere.obj")
    print(input.vertices)