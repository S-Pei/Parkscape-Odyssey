using System.Threading;
using System.Threading.Tasks;
using Microsoft.Geospatial;
using Microsoft.Maps.Unity;

public class ImageTextureTileLayer : TextureTileLayer {
    public string ImagePath;
    public override async Task<TextureTile?> GetTexture(TileId tileId, CancellationToken cancellationToken = default) {
        // Load image data from local file.
        var imageData = await Task.Run(() => {
            var path = ImagePath;
            var bytes = System.IO.File.ReadAllBytes(path);
            return bytes;
        });
        return TextureTile.FromImageData(imageData);
    }
}

// scaling considerations: when zoom in by 1 value, double the pixel count of the image -> same image but more detailed
// random textures: tree, pond, trail, house, etc.
// come back online?, check online status every update, disable and renable the map renderer if come back online?