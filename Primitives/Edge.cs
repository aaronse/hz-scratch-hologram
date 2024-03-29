﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScratchUtility;
using System.Drawing;

namespace Primitives
{ 
    /// <summary>Specifies the classification of the ViewEdge.</summary>
    public enum EdgeType
    {
        /// <summary>Specifies that the Edge connects two FrontFacing IndexedFaces</summary>
        FrontFacing,
        /// <summary>Specifies that the Edge connects two BackFacing IndexedFaces</summary>
        BackFacing,
        /// <summary>Specifies that the Edge connects a FrontFacing and a BackFacing IndexedFace</summary>
        Silhouette
    }

    /// <summary>Specifies whether or not this Edge connects two IndexedFaces, and if so, whether the Edge connection is Internal or External.</summary>
    public enum ConnectionType
    {
        /// <summary>Specifies that this Edge does not connect two IndexedFaces.</summary>
        None,
        /// <summary>Specifies that this Edge is an Internal Edge, meaning the angle between the IndexedFaces' NormalVectors is less than 180. Example: a room's corner viewed from the inside.</summary>
        Internal,
        /// <summary>Specifies that this Edge is an External Edge, meaning the angle between the IndexedFaces' NormalVectors is 180 or greater. Example: a house's corner viewed from the outside.</summary>
        External
    }

    /// <summary>Represents the edge shared by two IndexedFaces.</summary>
    public class Edge
    {
        private Coord? _unitVector = null;

        public static int s_duplicateEdges = 0;

#if DEBUG_USE_PROPS

    public IndexedFace CreatorFace { get; private set; }
    public IndexedFace OtherFace { get; private set; }
    public Vertex StartVertex { get; private set; }
    public Vertex EndVertex { get; private set; }
    public EdgeType Type { get; private set; }
    public ConnectionType ConnectionType { get; private set; }
    public List<EdgeSection> EdgeSections { get; set; }
    public List<Intersection> FaceIntersections { get; set; }
    public Rectangle BoundingBox { get; private set; }
    public Rect3 BoundingBox3 { get; set; }
    public int EdgeID { get; set; }

#else
        /// <summary>Gets the IndexedFace that created this Edge. Null for Auxiliary Faces. Knowing 
        /// which Face created the Edge helps to make sense of StartVertex and EndVertex: From 
        /// CreatorFace's point of view, StartVertex and EndVertex are in counter-clockwise order.
        /// </summary>
        public IndexedFace CreatorFace; //{ get; private set; }

        /// <summary>Gets the Face that did not create this Edge. From OtherFace's point of view, 
        /// StartVertex and EndVertex are in clockwise order instead of counter-clockwise as would 
        /// be expected when looking from the point of view of the CreatorFace.</summary>
        public IndexedFace OtherFace; //{ get; private set; }

        /// <summary>Gets the first Vertex supplied in the creation of this Edge. From CreatorFace's 
        /// point of view, StartVertex comes before EndVertex in counter-clockwise order.</summary>
        public Vertex StartVertex; // { get; private set; }

        /// <summary>Gets the second Vertex supplied in the creation of this Edge. From 
        /// CreatorFace's point of view, EndVertex comes after StartVertex in counter-clockwise 
        /// order.</summary>
        public Vertex EndVertex; // { get; private set; }

        /// <summary>Specifies the classification of the ViewEdge.</summary>
        public EdgeType Type; // { get; private set; }

        /// <summary>Specifies whether or not this Edge connects two IndexedFaces, and if so, 
        /// whether the Edge connection is Internal or External.</summary>
        public ConnectionType ConnectionType; // { get; private set; }

        /// <summary>Gets and sets the List of EdgeSections that make up this Edge. The StartVertex of the first EdgeSection is always either the Edge's StartVertex or EndVertex, depending on the direction of travel through the Edge. The EndVertex of the last EdgeSection is always this Edge's Vertex that is not the first EdgeSection's StartVertex.</summary>
        public List<EdgeSection> EdgeSections; // { get; set; }

        /// <summary>A List of all the coordinates along this Edge where it intersects with any IndexedFace. When splitting this Edge into EdgeSections, this list of Intersections is always used in addition to any Silhouette Edge intersections.</summary>
        public List<Intersection> FaceIntersections; // { get; set; }

        /// <summary>Gets the Rectangle that bounds this Edge when drawn on the sceen (the Vertices' ViewCoords are used).</summary>
        public Rectangle BoundingBox; // { get; private set; }

        public Rect3 BoundingBox3;

        public int EdgeID; // { get; set; }
#endif 



        public Edge(Vertex startVertex, Vertex endVertex, IndexedFace creatorFace)
        {
            StartVertex = startVertex;
            EndVertex = endVertex;
            CreatorFace = creatorFace;
            OtherFace = null;
            Type = EdgeType.FrontFacing; //Temporarily set to FrontFacing - its Type will be updated on Refresh().            
            EdgeSections = new List<EdgeSection>();
            FaceIntersections = new List<Intersection>();
            ConnectionType = ConnectionType.None;
            BoundingBox3 = new Rect3(startVertex.ModelingCoord, endVertex.ModelingCoord); 
        }

        /// <summary>Sets OtherFace to be the passed-in IndexedFace</summary>
        public void AddFace(IndexedFace face)
        {
            if (OtherFace != null)
            {
                // Monitor how oftern edge is referenced by more than 2 faces.  Mostly ignoring for now.
                // TODO:P2 Handle 3+ faces sharing the same Edge.
                //throw new Exception("Cannot set OtherFace on an Edge which already has an OtherFace");
                s_duplicateEdges++;
            }
                
            OtherFace = face;
        }

        /// <summary>Updates the ConnectionType of this Edge to accurately reflect the two 
        /// IndexedFaces that it connects. No update is made if this Edge does not have an 
        /// OtherFace. The ConnectionType never changes once set, so it is only necessary to call 
        /// this function once. Make sure both IndexedFaces have their NormalVectors updated 
        /// before calling this function.</summary>
        public void UpdateConnectionType()
        {
            if (OtherFace != null)
            {
                Coord diff = (EndVertex.ModelingCoord - StartVertex.ModelingCoord);
                Coord normal = OtherFace.NormalVector;
                Coord crossProduct = CreatorFace.NormalVector.CrossProduct(normal);
                double val = diff.DotProduct(crossProduct);
                
                if (double.IsNaN(val))
                {
                    ConnectionType = ConnectionType.External;
                }
                else
                {
                    ConnectionType = Math.Sign(val) == 1 ? ConnectionType.External : ConnectionType.Internal;

                }
            }
        }

        public void RefreshBoundingBox()
        {
            this.BoundingBox = Global.GetRectangleWithGivenCorners(StartVertex.ViewCoord.ToPointD(), EndVertex.ViewCoord.ToPointD());

            var startCoord = StartVertex.ModelingCoord;
            var endCoord = EndVertex.ModelingCoord;
        }

        /// <summary>Returns true if the specified Vertex is either the StartVertex or EndVertex of this Edge.</summary>
        public bool ContainsVertex(Vertex v)
        {
            return (v == StartVertex || v == EndVertex);
        }

        /// <summary>Returns true if the specified IndexedFace is either the CreatorFace or OtherFace of this Edge.</summary>
        public bool ContainsFace(IndexedFace ifc)
        {
            return (CreatorFace == ifc || OtherFace == ifc);
        }

        public double Length_ModelingCoordinates
        {
            get
            {
                return (EndVertex.ModelingCoord - StartVertex.ModelingCoord).CalcLength();
            }
        }

        public double Length_ViewCoordinates
        {
            get
            {
                return (EndVertex.ViewCoord - StartVertex.ViewCoord).CalcLength();
            }
        }

        /// <summary>Returns true if this Edge intersects the supplied Edge when drawn on the screen. ViewCoords are used. If there is a 2D projected intersection, the Z values at the intersection are compared. Returns True only if the Z value for the the SilhouetteEdge indicates that it is in front of this Edge.</summary>
        /// <param name="silhouetteEdge">The Edge to check against.</param>
        /// <returns>True if there was an intersection. Does not count intersections at the endpoints of the Edge.</returns>
        public bool IntersectsBehind(Edge silhouetteEdge, ref Coord intersectionPoint)
        {
            // Caching Unit Vectors, since profiler called out compute being a hot path.
            Coord unitVector;
            Coord silhouetteVector;
            if (!_unitVector.HasValue)
            {
                _unitVector = unitVector = (EndVertex.ModelingCoord - StartVertex.ModelingCoord).CalcUnitVector();
            }
            else
            {
                unitVector = _unitVector.Value;
            }

            if (!silhouetteEdge._unitVector.HasValue)
            {
                silhouetteEdge._unitVector = silhouetteVector = (silhouetteEdge.EndVertex.ModelingCoord - silhouetteEdge.StartVertex.ModelingCoord).CalcUnitVector();
            }
            else
            {
                silhouetteVector = silhouetteEdge._unitVector.Value;
            }

            if (Coord.IsEqualOrInverse(ref unitVector, ref silhouetteVector))
            {
                return false;
            }

            // Shortcut if the intersection occurs right at the corner of silhouetteEdge and this Edge.
            if (silhouetteEdge.StartVertex == StartVertex ||
                silhouetteEdge.EndVertex == StartVertex ||
                silhouetteEdge.StartVertex == EndVertex ||
                silhouetteEdge.EndVertex == EndVertex)
            {
                return false;
            }

            // If the BoundingBoxes don't intersect, then the Edges don't intersect
            if (!silhouetteEdge.BoundingBox.IntersectsWith(BoundingBox)) 
            {
                return false;
            }

            Coord startVertexViewCoord = this.StartVertex.ViewCoord;
            Coord endVertexViewCoord = this.EndVertex.ViewCoord;
            double a = startVertexViewCoord.X;
            double f = startVertexViewCoord.Y;
            double b = endVertexViewCoord.X;
            double g = endVertexViewCoord.Y;

            Coord silhouetteEdgeStartVertexViewCoord = silhouetteEdge.StartVertex.ViewCoord;
            Coord silhouetteEdgeEndVertexViewCoord = silhouetteEdge.EndVertex.ViewCoord;
            double c = silhouetteEdgeStartVertexViewCoord.X;
            double k = silhouetteEdgeStartVertexViewCoord.Y;
            double d = silhouetteEdgeEndVertexViewCoord.X;
            double l = silhouetteEdgeEndVertexViewCoord.Y;

            //Normalization technique will be used to determine if the lines intersect.
            //The following equations will be solved for t and u. If t and u are both between 0 and 1, the lines intersect
            // x1+(x2-x1)t = x3+(x4-x3)u
            // y1+(y2-y1)t = y3+(y4-y3)u
            double den = (a * (k - l) - b * (k - l) - (c - d) * (f - g));
            double t = (a * (k - l) - c * (f - l) + d * (f - k)) / den;
            double u = -(a * (g - k) - b * (f - k) + c * (f - g)) / den;

            if (Global.IsNotWithinTolerance(t) || Global.IsNotWithinTolerance(u))
            {
                return false;
            }

            //They intersect. Calculate the point of intersection.
            double zi = 
                startVertexViewCoord.Z + 
                (endVertexViewCoord.Z - startVertexViewCoord.Z) * t;
            double ziSilhouette = 
                silhouetteEdgeStartVertexViewCoord.Z + 
                (silhouetteEdgeEndVertexViewCoord.Z - silhouetteEdgeStartVertexViewCoord.Z) * u;
            if (zi > ziSilhouette) //if zi > ziSilhouette, the Silhouette Edge is behind this Edge, and thus the intersection can be ignored.
                return false;

            double xi = a + (b - a) * t;
            double yi = f + (g - f) * t;

            intersectionPoint.X = xi;
            intersectionPoint.Y = yi;
            intersectionPoint.Z = zi;
            return true;
        }


        public List<Coord> GetPoints(double viewPointsPerUnitLength, bool zeroAngle)
        {
            //Split the Edge up into coords
            int numPoints = Math.Max(2, (int)(viewPointsPerUnitLength * Length_ModelingCoordinates));

            List<Coord> points = new List<Coord>(numPoints);

            // Get Points along edge for current view angle.  Zero view angle points returned for
            // arcs and debug labels.
            Coord start = zeroAngle ? StartVertex.ViewCoord_ZeroAngle : StartVertex.ViewCoord;
            Coord end = zeroAngle ? EndVertex.ViewCoord_ZeroAngle : EndVertex.ViewCoord;

            for (int i = 0; i < numPoints; i++)
            {
                double fraction = i / (double)(numPoints - 1);
                points.Add(start + ((end - start) * (fraction)));
            }

            return points;
        }



        /// <summary>Refreshes this Edge so that its Type and BoundingBox accurately reflect the current IndexedFaceSet's AvailableViewVertexLocations.</summary>
        internal void Refresh()
        {
            //todo: check validity of what happens when there's only one edge
            if (OtherFace == null)
                Type = EdgeType.Silhouette;
            else if (CreatorFace.IsFrontFacing && OtherFace.IsFrontFacing)
                Type = EdgeType.FrontFacing;
            else if (!CreatorFace.IsFrontFacing && !OtherFace.IsFrontFacing)
                Type = EdgeType.BackFacing;
            else
                Type = EdgeType.Silhouette;
        }

        /// <summary>Returns the Vertex that is not the supplied Vertex. An Exception is thrown if the supplied Vertex is not either the StartVertex or EndVertex of this Edge.</summary>
        public Vertex GetOtherVertex(Vertex unwantedVertex)
        {
            if (unwantedVertex == StartVertex)
                return EndVertex;
            else if (unwantedVertex == EndVertex)
                return StartVertex;
            else
                throw new Exception("Can not Get Other Vertex because the supplied unwantedVertex is not part of this Edge.");
        }



        public string ToDebugString()
        {
            return StartVertex.VertexIndex + " to " + EndVertex.VertexIndex;
        }
    }
}
