# Semantria SDK for Python

The [Semantria](https://www.lexalytics.com/semantria) SDKs are the most convenient way to integrate with the Semantria API to build a continuous or high-volume application. The SDKs implement all available Semantria features and include some simple examples of their use. However, the examples are not intended to demonstrate the best practices for processing large volumes of data. Please contact Lexalytics for guidance if you plan to build your own application.

For small volume, or ad-hoc, interactive data exploration check out [Semantria for Excel](https://www.lexalytics.com/semantria/excel).

Signup for a free trial [here](https://www.lexalytics.com/signup).

See [semantria.readme.io](https://semantria.readme.io/docs/) for complete API documentation.

If you find a bug or have suggestions let us know: support@lexaltyics.com. Or fork this repo, make your changes, and submit a pull request.

## Installation

These instructions show how to build the SDK using python command line tools. You should be able to build and run the eamples in your Python IDE as well.

First, clone or download the SDK from https://github.com/Semantria/semantria-sdk.

Change directory into the Python directory. Then install the SDK:

    $ python setup.py

### Testing

To test, first set the environment variables SEMANTRIA\_KEY and SEMANTRIA\_SECRET with your key and secret.

Then run the tests:

    $ python tests/unittests.py

## Running the examples

There are several example programs in the examples subdirectory.

First, ensure the environment variables SEMANTRIA\_KEY and SEMANTRIA\_SECRET are set.

Change directory into the examples subdirectory. Then run an example (the detailed example in this case):

    $ python detailed_mode_test_app.py

Remember, these examples are coded in a simple style to illustrate the use of some of Semantria features. They do not represent the best practices for processing large volumes of data.

