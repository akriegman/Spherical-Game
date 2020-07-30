# This code will generate a list of vertices, edges, faces, and cells for the 600-cell and my 4800-cell

import math
import itertools
import csv

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

if __name__ == "__main__":
    # mode = "600"
    mode = "4800"
    test = False
    vertices = []
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

    edges = []
    faces = []
    cells = []
    if mode == "600":
        for u, v in itertools.combinations(vertices, 2):
            if dot(u, v) > 0.8:
                edges.append((u,v))
        for u, v, w in itertools.combinations(vertices, 3):
            if dot(u, v) > 0.8 and dot(v, w) > 0.8 and dot(w, u) > 0.8:
                faces.append((u,v,w))
        for u, v, w, x in itertools.combinations(vertices, 4):
            if  dot(u, v) > 0.8 and dot(v, w) > 0.8 and dot(w, u) > 0.8 and\
                dot(u, x) > 0.8 and dot(v, x) > 0.8 and dot(w, x) > 0.8:
                cells.append((u,v,w,x))
    if mode == "4800":
        # convert from string quaternions to tuple quaternions
        vertices = [strtotup(v) for v in vertices]
        # for efficiency we will keep new vertices separate
        newVertices = []
        # to keep track of the indices of new vertices
        nvi = len(vertices)
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
                newVertices.extend([vl1, vl2, vl3, vh1, vh2, vh3])
                il1, il2, il3, ih1, ih2, ih3 = nvi, nvi+1, nvi+2, nvi+3, nvi+4, nvi+5
                nvi += 6

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
                # TODO: create edges, faces, and other linkages once I figure what data structure I'm using
                # TODO: there are some duplicate vertices in here, I might want to fix that
        vertices.extend(newVertices)

        if test:
            rot = strtotup("/bp0")
            vertices = [prod(rot, v) for v in vertices]
            i1 = argmax([v[0] for v in vertices])
            testCells = cells[8*5*i1 + 32 : 8*5*i1 + 40]
            testCells = [vertices[i][1:4] for tet in testCells for i in tet]
            testFile = csv.writer(open("test/cells5.txt", "w"))
            testFile.writerows(testCells)