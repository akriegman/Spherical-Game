# This code will generate a list of vertices, edges, faces, and cells for the 600-cell and my 4800-cell

import math
import itertools
import csv
import numpy as np

vertices = []
edges = []
faces = []
cells = []
etof = []
ftoc = []

phi = (1 + math.sqrt(5))/2
rad = math.sqrt(10 + 2 * math.sqrt(5)) # appears in cos(18)
# convert from my symbolic notation to numerical values
# r s t u and v only used for 4800 cell
symEval = {'1':1, '0':0, '-':-1, '/':0.5, '\\':-0.5, 'p':phi/2, 'q':-phi/2, 'b':1/phi/2, 'd':-1/phi/2,
           'r':rad/4, 's':1/rad, 't':-1/rad, 'u':1/rad/phi, 'v':-1/rad/phi}
# for flipping the sign
symNeg = {'1':'-', '0':'0', '-':'1', '/':'\\', '\\':'/', 'p':'q', 'q':'p', 'b':'d', 'd':'b'}

# these 5 sets of 6 represent the five orientations of a cell
# one of each orientation is based at each vertex
# each of these quaternions is the sqrt of a 600-cell vertex nearest the identity
# first 3 in each set take you halfway from the base vertex to the other three vertices, 1 2 and 3 resp.
# next 3 take you halfway clockwise around the top of the tetrahedron, 1->2, 2->3, 3->1 resp.
# you must multiply with these on the left for proper orientation
# vertex 1 will be treated preferentially over the other 2 top vertices
# these are turned into tuples below
orients = [["r0su", "ru0s", "rsu0", "rsv0", "r0sv", "rv0s"],
           ["r0su", "rsu0", "r0sv", "ru0t", "rtu0", "ru0s"],
           ["r0su", "r0sv", "rtu0", "rv0t", "rv0s", "rsu0"],
           ["r0su", "rtu0", "rv0s", "rtv0", "ru0s", "r0sv"],
           ["r0su", "rv0s", "ru0s", "r0tu", "rsu0", "rtu0"]]

# dot product of string quaternions
def dot(q, p):
    return sum([symEval[x] * symEval[y] for x, y in zip(q, p)])

# dot product of tuple quaternions
def dott(q, p):
    return sum([x * y for x, y in zip(q, p)])

# tuple quaternion product of string quaternions
def prods(q, p):
    qw, qx, qy, qz = symEval[q[0]], symEval[q[1]], symEval[q[2]], symEval[q[3]]
    pw, px, py, pz = symEval[p[0]], symEval[p[1]], symEval[p[2]], symEval[p[3]]
    return (qw*pw - qx*px - qy*py - qz*pz,
            qw*px + qx*pw + qy*pz - qz*py,
            qw*py + qy*pw + qz*px - qx*pz,
            qw*pz + qz*pw + qx*py - qy*px)

# tuple quaternion product of tuple quaternions
def prod(q, p):
    return (q[0]*p[0] - q[1]*p[1] - q[2]*p[2] - q[3]*p[3],
            q[0]*p[1] + q[1]*p[0] + q[2]*p[3] - q[3]*p[2],
            q[0]*p[2] + q[2]*p[0] + q[3]*p[1] - q[1]*p[3],
            q[0]*p[3] + q[3]*p[0] + q[1]*p[2] - q[2]*p[1])

def inverse(q):
    n = q[0]*q[0] + q[1]*q[1] + q[2]*q[2] + q[3]*q[3]
    return (q[0]/n, -q[1]/n, -q[2]/n, -q[3]/n)

def normalize(v):
    return np.array(v) / np.linalg.norm(v)

# string quaternion to tuple quaternion
def strtotup(q):
    return tuple([symEval[q[i]] for i in range(4)])

# change orients from string to tuple
orients = [[strtotup(q) for q in tation] for tation in orients]

# recursive permutation functions
def oddPerms(l):
    if len(l) == 1:
        return
    for i in range(len(l)):
        s = l[i:i+1]
        t = l[:i] + l[i+1:]
        if i % 2:
            for p in evenPerms(t):
                yield s + p
        else:
            for p in oddPerms(t):
                yield s + p

def evenPerms(l):
    if len(l) == 1:
        yield l
        return
    for i in range(len(l)):
        s = l[i:i+1]
        t = l[:i] + l[i+1:]
        if i % 2:
            for p in oddPerms(t):
                yield s + p
        else:
            for p in evenPerms(t):
                yield s + p

def argmax(l):
    imax = 0
    max = l[0]
    for i in range(1, len(l)):
        if l[i] > max:
            imax = i
            max = l[i]
    return imax

# return the center of a list of vertices, not normalized
def center(l):
    s = np.array([sum([vertices[i][j] for i in l]) for j in range(4)])
    return s/len(l)

if __name__ == "__main__":
    # mode = "600"
    mode = "4800"
    # output = "test"
    output = "file"
    # the 8 axes
    for i in range(8):
        vertices.append("0001000-000"[i:i+4])
    # the 16 orthants
    for q in itertools.product("/\\", repeat=4):
        vertices.append("".join(q))
    # the 96 other vertices
    for q in evenPerms("p/b0"):
        for s in itertools.product("1-", repeat=4):
            # avoid duplicates
            for x, y in zip(q, s):
                if x == '0' and y == '-': break
            else:
                # flip some signs
                vertices.append("".join([symNeg[x] if y == '-' else x for x, y in zip(q,s)]))

    if mode == "600":
        for (i, u), (j, v) in itertools.combinations(enumerate(vertices), 2):
            if dot(u, v) > 0.8:
                edges.append((i,j))
        for (i, u), (j, v), (k, w) in itertools.combinations(enumerate(vertices), 3):
            if dot(u, v) > 0.8 and dot(v, w) > 0.8 and dot(w, u) > 0.8:
                faces.append((i,j,k))
        for (i, u), (j, v), (k, w), (l, x) in itertools.combinations(enumerate(vertices), 4):
            if  dot(u, v) > 0.8 and dot(v, w) > 0.8 and dot(w, u) > 0.8 and\
                dot(u, x) > 0.8 and dot(v, x) > 0.8 and dot(w, x) > 0.8:
                cells.append((i,j,k,l))

        vertices = [strtotup(v) for v in vertices]

    if mode == "4800":
        # convert from string quaternions to tuple quaternions
        vertices = [strtotup(v) for v in vertices]
        # for efficiency we will keep new vertices separate
        newVertices = []
        # to keep track of the indices of new vertices
        nvi = len(vertices)

        # helper function to eliminate duplicate vertices
        def addv(v):
            global nvi
            for i, u in enumerate(newVertices):
                if dott(u, v) > 0.97:
                    return i + len(vertices)
            else:
                newVertices.append(v)
                nvi += 1
                return nvi - 1

        # iterate over the 120 base vertices and 5 orientations
        # vertices starting with v are tuples, with i are indices
        for i0, v0 in enumerate(vertices):
            for tation in orients:
                # l is for little. these are halfway from the base vertex v0 to the respective main vertices
                vl1, vl2, vl3 = prod(tation[0], v0), prod(tation[1], v0), prod(tation[2], v0)
                # the main vertices. in case of numerical instability these are non-canonical
                v1, v2, v3 = prod(tation[0], vl1), prod(tation[1], vl2), prod(tation[2], vl3)
                i1 = argmax([dott(v1, v) for v in vertices])
                i2 = argmax([dott(v2, v) for v in vertices])
                i3 = argmax([dott(v3, v) for v in vertices])
                # replace the main vertices with their canonical values
                v1, v2, v3 = vertices[i1], vertices[i2], vertices[i3]
                # h is for halfway between neighboring vertices. between 1&2, 2&3, 3&1 resp.
                vh1, vh2, vh3 = prod(tation[3], v1), prod(tation[4], v2), prod(tation[5], v3)
                # throw our new vertices in the array and keep their indices
                # newVertices.extend([vl1, vl2, vl3, vh1, vh2, vh3])
                # il1, il2, il3, ih1, ih2, ih3 = nvi, nvi+1, nvi+2, nvi+3, nvi+4, nvi+5
                # nvi += 6
                il1, il2, il3, ih1, ih2, ih3 = addv(vl1), addv(vl2), addv(vl3), addv(vh1), addv(vh2), addv(vh3)

                # now we make the cells
                # vl1 and vh2 will be treated preferentially
                # it will help if you draw a picture for this
                cells.extend([[i0, il1, il2, il3],
                              [il1, i1, ih1, ih3],
                              [il2, i2, ih2, ih1],
                              [il3, i3, ih3, ih2],
                              [il1, ih2, il2, il3],
                              [il1, ih2, il3, ih3],
                              [il1, ih2, ih3, ih1],
                              [il1, ih2, ih1, il2]])

                edges.append([il1, ih2]) # the long edge is added now, other edges will be found later by length
                faces.extend([[il1, il2, ih2],
                              [il1, il3, ih2],
                              [il1, ih3, ih2],
                              [il1, ih1, ih2]]) # same deal here, these faces have a long edge

        vertices.extend(newVertices)

        # 0.93 will include all the actual edges except the long edges, which were handled above
        for (i, u), (j, v) in itertools.combinations(enumerate(vertices), 2):
            if dott(u, v) > 0.93:
                edges.append((i,j))
        for (i, u), (j, v), (k, w) in itertools.combinations(enumerate(vertices), 3):
            if dott(u, v) > 0.93 and dott(v, w) > 0.93 and dott(w, u) > 0.93:
                faces.append((i,j,k))

        if output == "test":
            rot = strtotup("/bp0")
            vertices = [prod(rot, v) for v in vertices]
            i1 = argmax([v[0] for v in vertices])
            testCells = cells[8*5*i1 + 32 : 8*5*i1 + 40]
            testCells = [vertices[i][1:4] for tet in testCells for i in tet]
            testFile = csv.writer(open("test/cells5.txt", "w"))
            testFile.writerows(testCells)
            testFile.close()

    # for each edge find all the faces using it
    for e in edges:
        usedby = []
        for i, f in enumerate(faces):
            if e[0] in f and e[1] in f:
                usedby.append(i)
        etof.append(usedby)
    # for each face find the cells using it
    for f in faces:
        usedby = []
        for i, c in enumerate(cells):
            if f[0] in c and f[1] in c and f[2] in c:
                usedby.append(i)
        ftoc.append(usedby)

    # sort the faces for each edge and the vertices for each face
    # right hand rule so that if you wrap your fingers around the faces for a given edge, your thumb points
    # from vertex 0 to vertex 1, likewise for vertices around a face and cell 0 to cell 1
    for i, e in enumerate(edges):
        # use this to send things to a neighborhood of 1
        frame = inverse(vertices[e[0]])
        # the spokes are centered around the pole
        pole = prod(frame, vertices[e[1]])
        spokes = [center(faces[j]) for j in etof[i]]
        spokes = [prod(frame, s) for s in spokes]
        # chop off w components as a crude projection
        pole = [pole[1], pole[2], pole[3]]
        spokes = [[s[1], s[2], s[3]] for s in spokes]
        # basis vectors
        pole = normalize(pole)
        e1 = np.array(spokes[0])
        e1 = e1 - pole * np.dot(pole, e1)
        e1 = normalize(e1)
        e2 = np.cross(pole, e1)
        # find the permutation
        perm = sorted(list(range(len(spokes))), key = lambda j: np.arctan2(np.dot(spokes[j], e2), np.dot(spokes[j], e1)))
        etof[i] = [etof[i][j] for j in perm]

    # repeat for faces
    for i, f in enumerate(faces):
        # use this to send things to a neighborhood of 1
        frame = inverse(center(cells[ftoc[i][0]]))
        # the spokes are centered around the pole
        pole = prod(frame, center(cells[ftoc[i][1]]))
        spokes = [vertices[j] for j in f]
        spokes = [prod(frame, s) for s in spokes]
        # chop off w components as a crude projection
        pole = [pole[1], pole[2], pole[3]]
        spokes = [[s[1], s[2], s[3]] for s in spokes]
        # basis vectors
        pole = normalize(pole)
        e1 = np.array(spokes[0])
        e1 = e1 - pole * np.dot(pole, e1)
        e1 = normalize(e1)
        e2 = np.cross(pole, e1)
        # find the permutation
        perm = sorted(list(range(len(spokes))), key = lambda j: np.arctan2(np.dot(spokes[j], e2), np.dot(spokes[j], e1)))
        faces[i] = [f[j] for j in perm]

    if output == "file":
        file = open("output/oriented4800cell.tope", "w+")
        for v in vertices:
            file.write("v: {:.6f} {:.6f} {:.6f} {:.6f}\n".format(v[1], v[2], v[3], v[0]))
        for i, e in enumerate(edges):
            file.write("e: {:d} {:d} :".format(e[0], e[1]))
            file.write("".join([" {}".format(j) for j in etof[i]]) + "\n")
        for i, f in enumerate(faces):
            file.write("f: {:d} {:d} {:d} :".format(f[0], f[1], f[2]))
            file.write("".join([" {}".format(j) for j in ftoc[i]]) + "\n")
        for c in cells:
            file.write("c: {:d} {:d} {:d} {:d}\n".format(c[0], c[1], c[2], c[3]))
        file.close()
