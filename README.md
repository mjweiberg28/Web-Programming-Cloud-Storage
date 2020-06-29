# Web-Programming-Cloud-Storage
Assignment 9 of Bethel University Fall 2019's Web Programming course

In this project I created an HTML page displaying a series of images, laid out
horizontally. The image elements are created dynamically as it loops through a
JSON response of image entities from the server. The user can upload an image,
but the user must also name the image and the image must be of valid size. Once
uploaded, a request is sent to notify the server and the new image is displayed.

- ImagesController
  - GET: Returns a JSON response with all the images on the server
  - GET(id): Returns a URL to an image as a temporary redirect. Cache is set for 7 hours.
  - POST: Accepts an ImageEntity JSON that allows the client to speficy the image
  name to be created. The response JSON has a URL and SAS token that can be used
  to upload an image to an azure storage account.
  - UploadComplete (POST): Accepts a string id that is an id of an image previously
  created. This updates the images that is already stored to save the upload that has completed.
