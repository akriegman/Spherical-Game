# This code will generate a list of vertices, edges, faces, and cells for the 600-cell and my 4800-cell

import math
import itertools

phi = (1 + math.sqrt(5))/2
# convert from my symbolic notation to numerical values
symEval = {'1':1, '0':0, '-':-1, '/':0.5, '\\':-0.5, 'p':phi/2, 'q':-phi/2, 'b':1/phi/2, 'd':-1/phi/2}
# for flipping the sign
symNeg = {'1':'-', '0':'0', '-':'1', '/':'\\', '\\':'/', 'p':'q', 'q':'p', 'b':'d', 'd':'b'}

def dot(q, p):
    return sum([symEval[x] * symEval[y] for x, y in zip(q, p)])

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

if __name__ == "__main__":
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