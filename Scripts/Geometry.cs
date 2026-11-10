using System;
namespace BubbleWrap;
public static class Geometry
{
    public static bool Hits(float ax,float ay,float bx,float by,float cx,float cy,float r)
    {
        float dx=bx-ax,dy=by-ay,len=dx*dx+dy*dy;
        float t=len==0?0:Math.Clamp(((cx-ax)*dx+(cy-ay)*dy)/len,0,1);
        float x=ax+t*dx-cx,y=ay+t*dy-cy;
        return x*x+y*y<=r*r;
    }
}
